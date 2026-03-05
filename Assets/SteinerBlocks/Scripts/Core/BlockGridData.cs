using System;
using UnityEngine;

namespace SteinerBlocks.Core
{
    /// <summary>
    /// Serializable data model for a grid of blocks.
    /// Compatible with the existing .blocks JSON format:
    /// {"BlockDataArray":[{"r":"0,90,0"},...],"Name":"001","Size":"50,20"}
    /// </summary>
    [Serializable]
    public class BlockGridData
    {
        public BlockRotationData[] BlockDataArray;
        public string Name;
        public string Size;

        public int Width
        {
            get
            {
                if (string.IsNullOrEmpty(Size)) return 0;
                var parts = Size.Split(',');
                return parts.Length >= 1 ? int.Parse(parts[0]) : 0;
            }
        }

        public int Height
        {
            get
            {
                if (string.IsNullOrEmpty(Size)) return 0;
                var parts = Size.Split(',');
                return parts.Length >= 2 ? int.Parse(parts[1]) : 0;
            }
        }

        /// <summary>
        /// Get rotation for block at grid position (column, row).
        /// Blocks are stored column-major: index = column * Height + row.
        /// </summary>
        public Quaternion GetRotation(int column, int row)
        {
            int index = column * Height + row;
            if (index < 0 || index >= BlockDataArray.Length)
                return Quaternion.identity;
            return BlockDataArray[index].ToQuaternion();
        }

        /// <summary>
        /// Set rotation for block at grid position (column, row).
        /// </summary>
        public void SetRotation(int column, int row, Quaternion rotation)
        {
            int index = column * Height + row;
            if (index < 0 || index >= BlockDataArray.Length) return;
            BlockDataArray[index] = BlockRotationData.FromEuler(rotation.eulerAngles);
        }

        /// <summary>
        /// Create a new grid with all blocks at identity rotation.
        /// </summary>
        public static BlockGridData Create(string name, int width, int height)
        {
            var data = new BlockGridData
            {
                Name = name,
                Size = $"{width},{height}",
                BlockDataArray = new BlockRotationData[width * height]
            };
            for (int i = 0; i < data.BlockDataArray.Length; i++)
            {
                data.BlockDataArray[i] = BlockRotationData.FromEuler(Vector3.zero);
            }
            return data;
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public static BlockGridData FromJson(string json)
        {
            return JsonUtility.FromJson<BlockGridData>(json);
        }
    }

    /// <summary>
    /// Serializable rotation data for a single block.
    /// Stored as comma-separated Euler angles: "x,y,z"
    /// </summary>
    [Serializable]
    public class BlockRotationData
    {
        public string r;

        public Vector3 ToEuler()
        {
            if (string.IsNullOrEmpty(r)) return Vector3.zero;
            var parts = r.Split(',');
            if (parts.Length < 3) return Vector3.zero;
            return new Vector3(
                float.Parse(parts[0]),
                float.Parse(parts[1]),
                float.Parse(parts[2]));
        }

        public Quaternion ToQuaternion()
        {
            return Quaternion.Euler(ToEuler());
        }

        public static BlockRotationData FromEuler(Vector3 euler)
        {
            return new BlockRotationData
            {
                r = $"{Mathf.Round(euler.x)},{Mathf.Round(euler.y)},{Mathf.Round(euler.z)}"
            };
        }
    }
}
