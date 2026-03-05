using System;
using System.IO;
using UnityEngine;
using SteinerBlocks.Core;

namespace SteinerBlocks.Persistence
{
    /// <summary>
    /// Cross-platform file I/O for block grid data.
    /// Replaces the Windows.Storage-based implementation with
    /// Application.persistentDataPath for all platforms.
    /// </summary>
    public static class BlockFileIO
    {
        /// <summary>
        /// Load block grid data from a .blocks file.
        /// First checks persistentDataPath (user saves), then falls back to Resources.
        /// </summary>
        public static BlockGridData LoadBlocks(string fileName)
        {
            // Try loading from persistent storage (user's saved data)
            string json = LoadFromPersistentData(fileName);

            // Fall back to bundled Resources
            if (string.IsNullOrEmpty(json))
            {
                json = LoadFromResources(fileName);
            }

            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"BlockFileIO: Could not load {fileName}");
                return null;
            }

            try
            {
                return BlockGridData.FromJson(json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"BlockFileIO: Failed to parse {fileName}: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Save block grid data to persistent storage.
        /// </summary>
        public static void SaveBlocks(BlockGridData data, string fileName)
        {
            if (!fileName.EndsWith(".blocks"))
                fileName += ".blocks";

            string path = Path.Combine(Application.persistentDataPath, fileName);
            try
            {
                string json = data.ToJson();
                File.WriteAllText(path, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"BlockFileIO: Failed to save {fileName}: {ex.Message}");
            }
        }

        /// <summary>
        /// Load all bundled pattern files from Resources/Blocks/.
        /// Returns array of JSON strings for slideshow use.
        /// </summary>
        public static string[] LoadBundledPatternJsons(string[] fileNames)
        {
            var jsons = new string[fileNames.Length];
            for (int i = 0; i < fileNames.Length; i++)
            {
                jsons[i] = LoadFromResources(fileNames[i]);
            }
            return jsons;
        }

        static string LoadFromPersistentData(string fileName)
        {
            if (!fileName.EndsWith(".blocks"))
                fileName += ".blocks";

            string path = Path.Combine(Application.persistentDataPath, fileName);
            if (File.Exists(path))
            {
                try
                {
                    return File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"BlockFileIO: Failed to read {path}: {ex.Message}");
                }
            }
            return null;
        }

        static string LoadFromResources(string fileName)
        {
            // Strip .blocks extension for Resources.Load
            string resourcePath = "Blocks/" + fileName.Replace(".blocks", "");
            var textAsset = Resources.Load<TextAsset>(resourcePath);
            if (textAsset != null)
            {
                return textAsset.text;
            }
            return null;
        }
    }
}
