using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
#endif

namespace Architecture.GameSound
{
    /// <summary>
    /// Holds the mapping between friendly clip ids and Addressables keys.
    /// Keeps data pure so the runtime service can stay completely stateless regarding authoring.
    /// </summary>
    [CreateAssetMenu(menuName = "16Party/Audio/Audio Catalog", fileName = "AudioCatalog")]
    public sealed class AudioCatalog : SerializedScriptableObject
    {
        [Title("Audio Configuration")]
        [DictionaryDrawerSettings(KeyLabel = "Clip Name (ID)", ValueLabel = "Addressable Key")]
        [SerializeField] 
        private Dictionary<string, string> _bgmDict = new Dictionary<string, string>();

        [DictionaryDrawerSettings(KeyLabel = "Clip Name (ID)", ValueLabel = "Addressable Key")]
        [SerializeField] 
        private Dictionary<string, string> _sfxDict = new Dictionary<string, string>();

        // Public Accessors
        public IReadOnlyDictionary<string, string> BGM => _bgmDict;
        public IReadOnlyDictionary<string, string> SFX => _sfxDict;

        public bool TryGetBGM(string id, out string addressKey) => _bgmDict.TryGetValue(id, out addressKey);
        public bool TryGetSFX(string id, out string addressKey) => _sfxDict.TryGetValue(id, out addressKey);

#if UNITY_EDITOR
        [Button("Auto Generate Index", ButtonSizes.Large), GUIColor(0, 1, 0)]
        private void GenerateIndex()
        {
            _bgmDict.Clear();
            _sfxDict.Clear();

            var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null)
            {
                Debug.LogError("[AudioCatalog] Addressable Asset Settings not found! Please create them first.");
                return;
            }

            ScanFolder("Assets/Audio/BGM", _bgmDict, settings);
            ScanFolder("Assets/Audio/SFX", _sfxDict, settings);
            
            GenerateConstantsFile();
            
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void GenerateConstantsFile()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("namespace Generated");
            sb.AppendLine("{");
            sb.AppendLine("    public static class AudioClipName");
            sb.AppendLine("    {");

            void WriteClass(string className, Dictionary<string, string> dict)
            {
                sb.AppendLine($"        public static class {className}");
                sb.AppendLine("        {");
                
                var usedNames = new HashSet<string>();
                foreach (var pair in dict)
                {
                    var id = pair.Key;
                    // Sanitize variable name: replace invalid chars with underscore
                    var varName = System.Text.RegularExpressions.Regex.Replace(id, @"[^a-zA-Z0-9_]", "_");
                    
                    // Ensure valid start char
                    if (string.IsNullOrEmpty(varName)) varName = "_";
                    if (char.IsDigit(varName[0])) varName = "_" + varName;

                    if (usedNames.Contains(varName))
                    {
                        Debug.LogWarning($"[AudioCatalog] Duplicate const name '{varName}' in {className}. Skipping.");
                        continue;
                    }
                    usedNames.Add(varName);

                    sb.AppendLine($"            public const string {varName} = \"{id}\";");
                }
                sb.AppendLine("        }");
            }

            WriteClass("BGM", _bgmDict);
            sb.AppendLine();
            WriteClass("SFX", _sfxDict);

            sb.AppendLine("    }");
            sb.AppendLine("}");

            var filePath = "Assets/Scripts/Generated/AudioClipName.cs";
            var dir = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(filePath, sb.ToString());
        }

        private void ScanFolder(string folderPath, Dictionary<string, string> targetDict, UnityEditor.AddressableAssets.Settings.AddressableAssetSettings settings)
        {
            if (!Directory.Exists(folderPath))
            {
                Debug.LogWarning($"[AudioCatalog] Folder not found: {folderPath}");
                return;
            }

            var guids = AssetDatabase.FindAssets("t:AudioClip", new[] { folderPath });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var entry = settings.FindAssetEntry(guid);

                if (entry == null)
                {
                    Debug.LogWarning($"[AudioCatalog] Asset not marked as Addressable: {path}");
                    continue;
                }

                // Use file name (without extension) as ID
                var id = Path.GetFileNameWithoutExtension(path);
                
                if (targetDict.ContainsKey(id))
                {
                    Debug.LogWarning($"[AudioCatalog] Duplicate audio ID found: {id} at {path}. Skipped.");
                    continue;
                }

                targetDict.Add(id, entry.address);
            }
        }
#endif
    }
}
