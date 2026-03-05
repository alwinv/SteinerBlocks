using UnityEngine;
using SteinerBlocks.Game;

namespace SteinerBlocks.Input
{
    /// <summary>
    /// Mouse and keyboard input handler for desktop/web builds and Unity Editor.
    /// Replaces HoloLens gesture recognizer and voice commands with
    /// standard mouse raycasting and keyboard shortcuts.
    ///
    /// This is the primary input method for Phase 1 (Editor testing)
    /// and the 2D web build. XR input is handled by XRInputHandler.
    /// </summary>
    public class DesktopInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] SelectionManager selectionManager;
        [SerializeField] GameManager gameManager;

        [Header("Settings")]
        [SerializeField] LayerMask blockLayer = ~0;
        [SerializeField] float maxRayDistance = 100f;

        // Drag rotation state
        bool isDragging;
        Vector3 dragStart;

        BlockController lastHovered;

        void Update()
        {
            HandleHover();
            HandleClick();
            HandleDragRotation();
            HandleKeyboardCommands();
        }

        /// <summary>
        /// Raycast from mouse position to detect block under cursor.
        /// </summary>
        void HandleHover()
        {
            if (selectionManager.SelectedBlock != null) return;
            if (isDragging) return;

            var cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(UnityEngine.Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, blockLayer))
            {
                var block = hit.collider.GetComponent<BlockController>();
                if (block != null)
                {
                    if (block != lastHovered)
                    {
                        if (lastHovered != null)
                            selectionManager.UnfocusBlock();

                        selectionManager.FocusBlock(block);
                        lastHovered = block;
                    }
                    return;
                }
            }

            // Nothing hit — clear focus
            if (lastHovered != null)
            {
                selectionManager.UnfocusBlock();
                lastHovered = null;
            }
        }

        /// <summary>
        /// Left click to select/deselect blocks.
        /// </summary>
        void HandleClick()
        {
            if (!UnityEngine.Input.GetMouseButtonDown(0)) return;

            selectionManager.ToggleSelection();
        }

        /// <summary>
        /// Right-click drag to rotate selected block.
        /// </summary>
        void HandleDragRotation()
        {
            if (selectionManager.SelectedBlock == null) return;

            // Begin drag
            if (UnityEngine.Input.GetMouseButtonDown(1))
            {
                isDragging = true;
                dragStart = UnityEngine.Input.mousePosition;
                selectionManager.BeginNavigation();
            }

            // During drag
            if (isDragging && UnityEngine.Input.GetMouseButton(1))
            {
                Vector3 delta = UnityEngine.Input.mousePosition - dragStart;
                // Normalize to roughly -1..1 range based on screen size
                Vector3 normalized = new Vector3(
                    delta.x / Screen.width,
                    delta.y / Screen.height,
                    0f);
                selectionManager.UpdateNavigation(normalized);
            }

            // End drag
            if (isDragging && UnityEngine.Input.GetMouseButtonUp(1))
            {
                isDragging = false;
                selectionManager.EndNavigation();
            }
        }

        /// <summary>
        /// Keyboard shortcuts for rotation and game commands.
        /// Maps to the same actions as the original voice commands.
        /// </summary>
        void HandleKeyboardCommands()
        {
            // Block rotation (when a block is selected)
            if (selectionManager.SelectedBlock != null)
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
                    selectionManager.RotateLeft();
                if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
                    selectionManager.RotateRight();
                if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
                    selectionManager.RotateUp();
                if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
                    selectionManager.RotateDown();
                if (UnityEngine.Input.GetKeyDown(KeyCode.R))
                    selectionManager.RotateTurn();
                if (UnityEngine.Input.GetKeyDown(KeyCode.F))
                    selectionManager.RotateFlip();
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) || UnityEngine.Input.GetKeyDown(KeyCode.Return))
                    selectionManager.DeselectBlock();
            }

            // Selection mode modifiers (hold while clicking)
            if (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))
            {
                if (UnityEngine.Input.GetKeyDown(KeyCode.C))
                    selectionManager.SelectFocused(SelectionManager.SelectionMode.Column);
                if (UnityEngine.Input.GetKeyDown(KeyCode.V))
                    selectionManager.SelectFocused(SelectionManager.SelectionMode.Row);
                if (UnityEngine.Input.GetKeyDown(KeyCode.B))
                    selectionManager.SelectFocused(SelectionManager.SelectionMode.All);
            }

            // Game commands
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha1))
                gameManager.ShowSlideshow();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha2))
                gameManager.HideSlideshow();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha3))
                gameManager.ShowLocalBlocks();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Alpha4))
                gameManager.HideLocalBlocks();
            if (UnityEngine.Input.GetKeyDown(KeyCode.Space))
            {
                if (gameManager.SlideshowRunning)
                    gameManager.PauseSlideshow();
                else
                    gameManager.ResumeSlideshow();
            }
        }
    }
}
