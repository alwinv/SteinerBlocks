using UnityEngine;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Controls a single block's visual state and animations.
    /// Replaces BlockBehaviors.cs — removes HoloToolkit IFocusable/IInputClickHandler.
    /// Selection/focus state is now driven by SelectionManager via public methods.
    /// </summary>
    public class BlockController : MonoBehaviour
    {
        // Grid position (set by BlockGridController when created)
        public int Column { get; set; }
        public int Row { get; set; }

        // Original transform values (for animation targets)
        Vector3 originalLocalPosition;
        Quaternion originalLocalRotation;
        Vector3 originalLocalScale;

        // Animation targets
        Vector3 targetPosition;
        Quaternion targetRotation;
        Vector3 targetScale;
        bool animating;

        // Animation speeds
        float moveSpeed = 0.2f;
        float rotateSpeed = 360f;
        float scaleSpeed = 100f;

        // Navigation rotation reference frame
        Transform axesTransform;
        Quaternion navStartRotation;

        void Start()
        {
            originalLocalPosition = transform.localPosition;
            originalLocalRotation = transform.localRotation;
            originalLocalScale = transform.localScale;

            targetPosition = originalLocalPosition;
            targetRotation = originalLocalRotation;
            targetScale = originalLocalScale;

            // Create a child-of-parent transform for rotation reference
            axesTransform = new GameObject("RotationAxes").transform;
            axesTransform.SetParent(transform.parent);

            AssignRandomTexture();
        }

        void Update()
        {
            if (!animating) return;

            float dt = Time.deltaTime;

            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, targetPosition, dt * moveSpeed);
            transform.localRotation = Quaternion.RotateTowards(
                transform.localRotation, targetRotation, dt * rotateSpeed);
            transform.localScale = Vector3.MoveTowards(
                transform.localScale, targetScale, dt * scaleSpeed);

            if (transform.localRotation == targetRotation
                && transform.localPosition == targetPosition
                && transform.localScale == targetScale)
            {
                animating = false;
            }
        }

        void AssignRandomTexture()
        {
            if (GameManager.Instance == null) return;

            var renderer = GetComponent<Renderer>();
            if (renderer == null) return;

            int textureIndex = GameManager.Instance.Rng.Next(26);
            string texName = $"Textures/block_{textureIndex:D3}";

            var mainTex = Resources.Load<Texture>(texName);
            var bumpTex = Resources.Load<Texture>(texName + "_gray");

            if (mainTex != null)
                renderer.material.SetTexture("_MainTex", mainTex);
            if (bumpTex != null)
                renderer.material.SetTexture("_BumpMap", bumpTex);
        }

        #region Focus & Selection (called by SelectionManager)

        /// <summary>
        /// Called when this block gains focus (gaze/hover).
        /// Lifts the block slightly off the grid.
        /// </summary>
        public void OnFocusEnter()
        {
            targetPosition = new Vector3(
                transform.localPosition.x,
                originalLocalPosition.y + 0.2f * GameManager.BlockSpacing,
                transform.localPosition.z);
            moveSpeed = 2f;
            animating = true;
        }

        /// <summary>
        /// Called when this block loses focus.
        /// Returns the block to its grid position.
        /// </summary>
        public void OnFocusExit()
        {
            targetPosition = originalLocalPosition;
            moveSpeed = 0.2f;
            animating = true;
        }

        /// <summary>
        /// Called when this block is selected for editing.
        /// Pops out and scales up.
        /// </summary>
        public void OnSelect()
        {
            targetPosition = new Vector3(
                transform.localPosition.x,
                originalLocalPosition.y + 4f * GameManager.BlockSpacing,
                transform.localPosition.z);
            targetScale = originalLocalScale * GameManager.SelectedBlockScale;
            moveSpeed = 2f;
            animating = true;

            PlayClickSound();
        }

        /// <summary>
        /// Called when this block is deselected.
        /// Returns to original position and scale.
        /// </summary>
        public void OnDeselect()
        {
            targetPosition = originalLocalPosition;
            targetScale = originalLocalScale;
            moveSpeed = 2f;
            animating = true;

            PlayClickSound();
        }

        #endregion

        #region Rotation Commands

        /// <summary>
        /// Initialize the rotation reference frame based on camera direction.
        /// Must be called before relative rotations.
        /// </summary>
        public void InitRotationFrame()
        {
            var cam = Camera.main;
            if (cam == null) return;

            // Create temp transform looking at the block from camera position
            var temp = new GameObject("TempLookAt").transform;
            temp.SetParent(transform.parent);
            temp.rotation = cam.transform.rotation;
            temp.position = cam.transform.position;
            temp.LookAt(transform, cam.transform.up);

            // Snap to nearest 90-degree angles
            Vector3 snapped;
            snapped.x = Mathf.RoundToInt(temp.localEulerAngles.x / 90f) * 90f;
            snapped.y = Mathf.RoundToInt(temp.localEulerAngles.y / 90f) * 90f;
            snapped.z = Mathf.RoundToInt(temp.localEulerAngles.z / 90f) * 90f;
            temp.localEulerAngles = snapped;

            axesTransform.rotation = temp.rotation;
            axesTransform.position = temp.position;
            navStartRotation = transform.rotation;

            Destroy(temp.gameObject);
        }

        /// <summary>
        /// Apply a relative rotation based on drag/navigation offset.
        /// </summary>
        public void RotateRelative(Vector3 amount)
        {
            animating = false;
            float factor = 180f; // NavigationToRotationFactor

            transform.rotation = navStartRotation;

            if (Mathf.Abs(amount.y) > 0)
                transform.Rotate(axesTransform.right, amount.y * factor, Space.World);
            if (Mathf.Abs(amount.x) > 0)
                transform.Rotate(axesTransform.up, amount.x * factor, Space.World);
            if (Mathf.Abs(amount.z) > 0)
                transform.Rotate(axesTransform.forward, -amount.z * factor, Space.World);
        }

        /// <summary>
        /// Snap rotation to the nearest 90-degree increment (after drag ends).
        /// </summary>
        public void SnapRotation()
        {
            Vector3 snapped;
            snapped.x = Mathf.RoundToInt(transform.localEulerAngles.x / 90f) * 90f;
            snapped.y = Mathf.RoundToInt(transform.localEulerAngles.y / 90f) * 90f;
            snapped.z = Mathf.RoundToInt(transform.localEulerAngles.z / 90f) * 90f;

            targetRotation = Quaternion.Euler(snapped);
            animating = true;
        }

        /// <summary>
        /// Set rotation to an absolute value (used by slideshow).
        /// </summary>
        public void SetRotationAnimated(Quaternion localRotation)
        {
            targetRotation = localRotation;
            animating = true;
        }

        // Voice/keyboard rotation commands
        public void RotateLeft()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.up, 90f);
            animating = true;
        }

        public void RotateRight()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.up, -90f);
            animating = true;
        }

        public void RotateUp()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.right, 90f);
            animating = true;
        }

        public void RotateDown()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.right, -90f);
            animating = true;
        }

        public void RotateTurn()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.forward, -90f);
            animating = true;
        }

        public void RotateFlip()
        {
            InitRotationFrame();
            targetRotation = ComputeRotation(axesTransform.forward, 180f);
            animating = true;
        }

        Quaternion ComputeRotation(Vector3 axis, float angle)
        {
            var temp = new GameObject("TempRotation").transform;
            temp.SetParent(transform.parent);
            temp.rotation = transform.rotation;
            temp.RotateAround(temp.position, axis, angle);
            Quaternion result = temp.localRotation;
            Destroy(temp.gameObject);
            return result;
        }

        #endregion

        void PlayClickSound()
        {
            var audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
                audioSource.Play();
        }
    }
}
