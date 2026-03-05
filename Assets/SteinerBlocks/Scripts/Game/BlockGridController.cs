using UnityEngine;
using SteinerBlocks.Core;
using SteinerBlocks.Persistence;

namespace SteinerBlocks.Game
{
    /// <summary>
    /// Manages a grid of block GameObjects.
    /// Replaces the grid creation and slideshow logic from BlockIO.cs.
    /// No Windows-specific APIs, no #if !UNITY_EDITOR blocks.
    /// </summary>
    public class BlockGridController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] GameObject blockPrefab;
        [SerializeField] Transform gridParent;

        [Header("Settings")]
        [SerializeField] float scaleFactor = 1f;
        [SerializeField] bool isEditable = true;

        // Current grid data
        BlockGridData currentData;

        // Slideshow state
        string[] slideshowJsons;
        int slideshowIndex;

        // Block references (column-major: [column * height + row])
        BlockController[] blockControllers;

        public bool IsEditable => isEditable;
        public BlockGridData CurrentData => currentData;
        public int BlockCount => blockControllers != null ? blockControllers.Length : 0;

        /// <summary>
        /// Load a single .blocks file and create the grid.
        /// </summary>
        public void LoadFile(string fileName)
        {
            var data = BlockFileIO.LoadBlocks(fileName);
            if (data == null)
            {
                Debug.LogWarning($"BlockGridController: Failed to load {fileName}");
                return;
            }

            currentData = data;
            CreateGrid(data);
            transform.localScale = Vector3.one * scaleFactor;
        }

        /// <summary>
        /// Load multiple .blocks files for slideshow mode.
        /// Creates the grid from the first file.
        /// </summary>
        public void LoadPatternFiles(string[] fileNames)
        {
            slideshowJsons = BlockFileIO.LoadBundledPatternJsons(fileNames);
            slideshowIndex = 0;

            if (slideshowJsons.Length > 0 && !string.IsNullOrEmpty(slideshowJsons[0]))
            {
                currentData = BlockGridData.FromJson(slideshowJsons[0]);
                CreateGrid(currentData);
                transform.localScale = Vector3.one * scaleFactor;
            }
        }

        /// <summary>
        /// Advance to the next pattern in slideshow mode.
        /// Rotates block GameObjects to match the new pattern.
        /// </summary>
        public void ShowNextPattern()
        {
            if (slideshowJsons == null || slideshowJsons.Length == 0) return;

            slideshowIndex = (slideshowIndex + 1) % slideshowJsons.Length;
            string json = slideshowJsons[slideshowIndex];
            if (string.IsNullOrEmpty(json)) return;

            currentData = BlockGridData.FromJson(json);

            // Animate existing blocks to new rotations
            if (blockControllers != null && currentData.BlockDataArray != null)
            {
                int count = Mathf.Min(blockControllers.Length, currentData.BlockDataArray.Length);
                for (int i = 0; i < count; i++)
                {
                    if (blockControllers[i] != null)
                    {
                        var rotation = currentData.BlockDataArray[i].ToQuaternion();
                        blockControllers[i].SetRotationAnimated(rotation);
                    }
                }
            }
        }

        /// <summary>
        /// Save the current grid state to persistent storage.
        /// </summary>
        public void SaveToFile()
        {
            if (currentData == null || blockControllers == null) return;

            // Update data from current block rotations
            int width = currentData.Width;
            int height = currentData.Height;

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int index = col * height + row;
                    if (index < blockControllers.Length && blockControllers[index] != null)
                    {
                        var euler = blockControllers[index].transform.localEulerAngles;
                        currentData.BlockDataArray[index] = BlockRotationData.FromEuler(euler);
                    }
                }
            }

            BlockFileIO.SaveBlocks(currentData, currentData.Name);
        }

        /// <summary>
        /// Show or hide all block renderers.
        /// </summary>
        public void SetVisible(bool visible)
        {
            var renderers = GetComponentsInChildren<MeshRenderer>();
            foreach (var r in renderers)
            {
                if (r != null) r.enabled = visible;
            }

            isEditable = visible && isEditable;
        }

        /// <summary>
        /// Get the BlockController at a specific grid position.
        /// </summary>
        public BlockController GetBlock(int column, int row)
        {
            if (currentData == null) return null;
            int index = column * currentData.Height + row;
            if (index < 0 || blockControllers == null || index >= blockControllers.Length)
                return null;
            return blockControllers[index];
        }

        /// <summary>
        /// Get the BlockController by its sibling index in the grid parent.
        /// </summary>
        public BlockController GetBlockByIndex(int index)
        {
            if (blockControllers == null || index < 0 || index >= blockControllers.Length)
                return null;
            return blockControllers[index];
        }

        /// <summary>
        /// Get all blocks in a given column.
        /// </summary>
        public BlockController[] GetColumn(int column)
        {
            if (currentData == null) return null;
            int h = currentData.Height;
            var result = new BlockController[h];
            for (int row = 0; row < h; row++)
            {
                result[row] = GetBlock(column, row);
            }
            return result;
        }

        /// <summary>
        /// Get all blocks in a given row.
        /// </summary>
        public BlockController[] GetRow(int row)
        {
            if (currentData == null) return null;
            int w = currentData.Width;
            var result = new BlockController[w];
            for (int col = 0; col < w; col++)
            {
                result[col] = GetBlock(col, row);
            }
            return result;
        }

        /// <summary>
        /// Get all blocks in the grid.
        /// </summary>
        public BlockController[] GetAllBlocks()
        {
            return blockControllers;
        }

        void CreateGrid(BlockGridData data)
        {
            // Clear existing blocks
            if (gridParent != null)
            {
                for (int i = gridParent.childCount - 1; i >= 0; i--)
                {
                    Destroy(gridParent.GetChild(i).gameObject);
                }
            }

            if (data == null || data.BlockDataArray == null) return;

            int width = data.Width;
            int height = data.Height;
            float spacing = GameManager.BlockSpacing;
            float colOffset = width / 2f;

            blockControllers = new BlockController[width * height];

            for (int col = 0; col < width; col++)
            {
                for (int row = 0; row < height; row++)
                {
                    int index = col * height + row;
                    var rotation = data.BlockDataArray[index].ToQuaternion();

                    GameObject block = Instantiate(blockPrefab, gridParent, false);
                    block.transform.localRotation = rotation;
                    block.transform.localPosition = new Vector3(
                        (col - colOffset) * spacing,
                        1f * spacing,
                        (row + 1) * spacing);
                    block.name = $"{data.Name}_{row}-{col}";

                    var controller = block.GetComponent<BlockController>();
                    if (controller == null)
                        controller = block.AddComponent<BlockController>();

                    controller.Column = col;
                    controller.Row = row;
                    blockControllers[index] = controller;
                }
            }
        }
    }
}
