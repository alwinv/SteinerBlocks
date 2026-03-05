using System;
using UnityEngine;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Manages block selection state and dispatches rotation commands.
    /// Extracts and centralizes the selection logic that was spread across
    /// BlockBehaviors.cs, TappedHandler.cs, and Globals.cs.
    /// </summary>
    public class SelectionManager : MonoBehaviour
    {
        public enum SelectionMode
        {
            Block, Row, Column, All
        }

        // Current state
        public BlockController FocusedBlock { get; private set; }
        public BlockController SelectedBlock { get; private set; }
        public SelectionMode CurrentMode { get; private set; } = SelectionMode.Block;
        public BlockGridController ActiveGrid { get; set; }

        // Navigation state
        public bool IsNavigating { get; private set; }
        Vector3 navigatingDirection = Vector3.zero;

        // Events
        public event Action<BlockController> OnBlockFocused;
        public event Action<BlockController> OnBlockUnfocused;
        public event Action<BlockController> OnBlockSelected;
        public event Action<BlockController> OnBlockDeselected;

        /// <summary>
        /// Set focus on a block (hover/gaze).
        /// </summary>
        public void FocusBlock(BlockController block)
        {
            if (IsNavigating || SelectedBlock != null) return;
            if (ActiveGrid != null && !ActiveGrid.IsEditable) return;

            if (FocusedBlock != null && FocusedBlock != block)
                UnfocusBlock();

            FocusedBlock = block;
            block.OnFocusEnter();
            OnBlockFocused?.Invoke(block);
        }

        /// <summary>
        /// Remove focus from the current block.
        /// </summary>
        public void UnfocusBlock()
        {
            if (FocusedBlock == null) return;
            if (IsNavigating || SelectedBlock != null) return;

            FocusedBlock.OnFocusExit();
            OnBlockUnfocused?.Invoke(FocusedBlock);
            FocusedBlock = null;
        }

        /// <summary>
        /// Select a block for editing.
        /// </summary>
        public void SelectBlock(BlockController block, SelectionMode mode = SelectionMode.Block)
        {
            if (block == null) return;
            if (ActiveGrid != null && !ActiveGrid.IsEditable) return;

            // Deselect current first
            if (SelectedBlock != null)
            {
                DeselectBlock();
            }

            SelectedBlock = block;
            CurrentMode = mode;
            block.OnSelect();
            OnBlockSelected?.Invoke(block);
        }

        /// <summary>
        /// Select the currently focused block.
        /// </summary>
        public void SelectFocused(SelectionMode mode = SelectionMode.Block)
        {
            if (FocusedBlock != null)
            {
                SelectBlock(FocusedBlock, mode);
            }
        }

        /// <summary>
        /// Deselect the current block.
        /// </summary>
        public void DeselectBlock()
        {
            if (SelectedBlock == null) return;

            SelectedBlock.OnDeselect();
            OnBlockDeselected?.Invoke(SelectedBlock);
            SelectedBlock = null;

            // Trigger save
            if (ActiveGrid != null)
                ActiveGrid.SaveToFile();
        }

        /// <summary>
        /// Toggle selection: if the focused block is selected, deselect; otherwise select.
        /// </summary>
        public void ToggleSelection()
        {
            if (SelectedBlock != null)
            {
                DeselectBlock();
            }
            else if (FocusedBlock != null)
            {
                SelectBlock(FocusedBlock);
            }
        }

        #region Rotation Commands

        /// <summary>
        /// Apply a rotation command to all selected blocks (respects selection mode).
        /// </summary>
        public void RotateSelected(System.Action<BlockController> rotateAction)
        {
            if (SelectedBlock == null) return;
            ForEachSelectedBlock(rotateAction);
        }

        public void RotateLeft() => RotateSelected(b => b.RotateLeft());
        public void RotateRight() => RotateSelected(b => b.RotateRight());
        public void RotateUp() => RotateSelected(b => b.RotateUp());
        public void RotateDown() => RotateSelected(b => b.RotateDown());
        public void RotateTurn() => RotateSelected(b => b.RotateTurn());
        public void RotateFlip() => RotateSelected(b => b.RotateFlip());

        #endregion

        #region Drag/Navigation Rotation

        public void BeginNavigation()
        {
            if (SelectedBlock == null) return;
            IsNavigating = true;
            navigatingDirection = Vector3.zero;
        }

        public void UpdateNavigation(Vector3 normalizedOffset)
        {
            if (SelectedBlock == null || !IsNavigating) return;

            if (navigatingDirection == Vector3.zero)
            {
                if (TryLockDirection(normalizedOffset))
                {
                    ForEachSelectedBlock(b => b.InitRotationFrame());
                }
            }
            else
            {
                Vector3 constrained = Vector3.Scale(normalizedOffset, navigatingDirection);
                ForEachSelectedBlock(b => b.RotateRelative(constrained));
            }
        }

        public void EndNavigation()
        {
            if (SelectedBlock == null) return;
            IsNavigating = false;
            navigatingDirection = Vector3.zero;
            ForEachSelectedBlock(b => b.SnapRotation());
        }

        bool TryLockDirection(Vector3 offset)
        {
            float threshold = 0.1f;
            float ax = Mathf.Abs(offset.x);
            float ay = Mathf.Abs(offset.y);
            float az = Mathf.Abs(offset.z);

            if (ax < threshold && ay < threshold && az < threshold)
                return false;

            if (ax >= ay && ax >= az)
                navigatingDirection = Vector3.left;
            else if (ay >= ax && ay >= az)
                navigatingDirection = Vector3.up;
            else
                navigatingDirection = Vector3.forward;

            return true;
        }

        #endregion

        #region Selection Mode Helpers

        /// <summary>
        /// Apply an action to all blocks affected by the current selection mode.
        /// </summary>
        void ForEachSelectedBlock(System.Action<BlockController> action)
        {
            if (SelectedBlock == null || ActiveGrid == null) return;

            switch (CurrentMode)
            {
                case SelectionMode.Block:
                    action(SelectedBlock);
                    break;

                case SelectionMode.Column:
                    var colBlocks = ActiveGrid.GetColumn(SelectedBlock.Column);
                    if (colBlocks != null)
                        foreach (var b in colBlocks)
                            if (b != null) action(b);
                    break;

                case SelectionMode.Row:
                    var rowBlocks = ActiveGrid.GetRow(SelectedBlock.Row);
                    if (rowBlocks != null)
                        foreach (var b in rowBlocks)
                            if (b != null) action(b);
                    break;

                case SelectionMode.All:
                    var allBlocks = ActiveGrid.GetAllBlocks();
                    if (allBlocks != null)
                        foreach (var b in allBlocks)
                            if (b != null) action(b);
                    break;
            }
        }

        #endregion
    }
}
