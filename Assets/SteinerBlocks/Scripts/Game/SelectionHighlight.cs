using UnityEngine;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Visual highlight that follows focused/selected blocks.
    /// Modernized from SelectionHighlightCommands.cs.
    /// Uses transform.Find() instead of deprecated FindChild().
    /// Subscribes to SelectionManager events instead of using SendMessage().
    /// </summary>
    public class SelectionHighlight : MonoBehaviour
    {
        MeshRenderer[] focusRenderers;
        MeshRenderer[] selectionRenderers;

        Vector3 originalPosition;
        Vector3 originalScale;
        bool isSelected;

        SelectionManager selectionManager;

        void Start()
        {
            originalPosition = transform.localPosition;
            originalScale = transform.localScale;

            // Find child highlight meshes
            var focusChild = transform.Find("FocusHighlight");
            if (focusChild != null)
                focusRenderers = focusChild.GetComponentsInChildren<MeshRenderer>();

            var selectionChild = transform.Find("SelectionHighlight");
            if (selectionChild != null)
                selectionRenderers = selectionChild.GetComponentsInChildren<MeshRenderer>();

            // Hide all highlights initially
            SetRenderersEnabled(focusRenderers, false);
            SetRenderersEnabled(selectionRenderers, false);

            // Subscribe to selection events
            if (GameManager.Instance != null)
            {
                selectionManager = GameManager.Instance.selectionManager;
                if (selectionManager != null)
                {
                    selectionManager.OnBlockFocused += HandleFocused;
                    selectionManager.OnBlockUnfocused += HandleUnfocused;
                    selectionManager.OnBlockSelected += HandleSelected;
                    selectionManager.OnBlockDeselected += HandleDeselected;
                }
            }
        }

        void OnDestroy()
        {
            if (selectionManager != null)
            {
                selectionManager.OnBlockFocused -= HandleFocused;
                selectionManager.OnBlockUnfocused -= HandleUnfocused;
                selectionManager.OnBlockSelected -= HandleSelected;
                selectionManager.OnBlockDeselected -= HandleDeselected;
            }
        }

        void Update()
        {
            if (!isSelected) return;
            if (selectionManager == null || selectionManager.SelectedBlock == null) return;

            var selected = selectionManager.SelectedBlock;

            // Follow the selected block's position
            transform.localPosition = selected.transform.localPosition;

            // Orient highlight to face the camera
            var cam = Camera.main;
            if (cam == null) return;

            var temp = new GameObject("TempHighlightLook").transform;
            temp.SetParent(selected.transform.parent);
            temp.rotation = cam.transform.rotation;
            temp.position = cam.transform.position;
            temp.LookAt(selected.transform, cam.transform.up);

            Vector3 snapped;
            snapped.x = Mathf.RoundToInt(temp.localEulerAngles.x / 90f) * 90f;
            snapped.y = Mathf.RoundToInt(temp.localEulerAngles.y / 90f) * 90f;
            snapped.z = Mathf.RoundToInt(temp.localEulerAngles.z / 90f) * 90f;
            temp.localEulerAngles = snapped;

            transform.rotation = temp.rotation;
            transform.Rotate(temp.right, -90f, Space.World);
            transform.position = selected.transform.position;
            transform.Translate(0f, 0f, -2f * GameManager.BlockSpacing, temp);

            Destroy(temp.gameObject);
        }

        void HandleFocused(BlockController block)
        {
            SetRenderersEnabled(focusRenderers, true);

            // Move highlight to the focused block's grid
            transform.SetParent(block.transform.parent);
            transform.localScale = originalScale;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = new Vector3(
                block.transform.localPosition.x,
                block.transform.localPosition.y + 0.5f * GameManager.BlockSpacing,
                block.transform.localPosition.z);
        }

        void HandleUnfocused(BlockController block)
        {
            SetRenderersEnabled(focusRenderers, false);
            transform.localPosition = originalPosition;
        }

        void HandleSelected(BlockController block)
        {
            SetRenderersEnabled(focusRenderers, false);
            SetRenderersEnabled(selectionRenderers, true);
            transform.localScale = originalScale * GameManager.SelectedBlockScale;
            transform.localRotation = Quaternion.Euler(0f, 45f, 0f);
            isSelected = true;
        }

        void HandleDeselected(BlockController block)
        {
            SetRenderersEnabled(focusRenderers, false);
            SetRenderersEnabled(selectionRenderers, false);
            transform.localScale = originalScale;
            transform.localRotation = Quaternion.identity;
            transform.localPosition = originalPosition;
            isSelected = false;
        }

        static void SetRenderersEnabled(MeshRenderer[] renderers, bool enabled)
        {
            if (renderers == null) return;
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = enabled;
            }
        }
    }
}
