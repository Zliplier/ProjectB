using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// A single customization entry: a color and/or icon assigned to either
    /// a GameObject (keyed by a stable GlobalObjectId string) or a Project
    /// folder (keyed by its asset GUID).
    /// </summary>
    [Serializable]
    public class CustomizerEntry
    {
        public string key;

        public bool hasColor;
        public Color color = new Color(0.35f, 0.55f, 0.85f, 0.35f);

        public bool hasIcon;
        public string iconName;   // built-in icon name, OR a texture GUID when isCustomIcon is true
        public bool isCustomIcon;
    }

    /// <summary>
    /// Persistent ScriptableObject asset that stores every customization made
    /// in the Hierarchy and Project windows.
    ///
    /// This is intentionally NOT auto-created. Create it once via
    /// Tools > Hierarchy Customizer > Create Database Asset, then commit the
    /// resulting .asset (and its .meta) to source control. Auto-creating it
    /// on first use meant every machine that opened the project before
    /// pulling the committed asset would generate its own copy with a
    /// different GUID, which produced conflicts and broken references on
    /// clone/merge - exactly what this is meant to avoid.
    ///
    /// The asset is located by type anywhere in the project (not by a fixed
    /// path), so it's found correctly regardless of where you put it or
    /// where a git clone puts it.
    /// </summary>
    public class CustomizerDatabase : ScriptableObject
    {
        private const string AssetFileName = "CustomizerDatabase.asset";
        private const string NoDatabaseWarnedKey = "HierarchyCustomizer_NoDatabaseWarned";

        [SerializeField] private List<CustomizerEntry> entries = new List<CustomizerEntry>();

        private Dictionary<string, CustomizerEntry> lookup;
        private static CustomizerDatabase instance;
        private static bool searchedForInstance;
        private static string cachedFolder;

        /// <summary>
        /// The active database, or null if none has been created yet. Never
        /// creates one as a side effect of being read.
        /// </summary>
        public static CustomizerDatabase Instance
        {
            get
            {
                if (instance != null) return instance;
                if (searchedForInstance) return null;

                instance = FindExisting();
                searchedForInstance = true;

                if (instance == null && !SessionState.GetBool(NoDatabaseWarnedKey, false))
                {
                    SessionState.SetBool(NoDatabaseWarnedKey, true);
                    Debug.Log("[HierarchyCustomizer] No database asset found - color/icon customization " +
                              "is inactive until you create one. Use Tools > Hierarchy Customizer > " +
                              "Create Database Asset, then commit the resulting asset to source control.");
                }

                return instance;
            }
        }

        public IReadOnlyList<CustomizerEntry> Entries => entries;

        private static CustomizerDatabase FindExisting()
        {
            var guids = AssetDatabase.FindAssets("t:" + nameof(CustomizerDatabase));
            if (guids.Length == 0)
                return null;

            if (guids.Length > 1)
            {
                Debug.LogWarning($"[HierarchyCustomizer] Found {guids.Length} CustomizerDatabase assets in " +
                                  "the project - using the first one. Search the Project window for " +
                                  "\"t:CustomizerDatabase\" to find and delete the extras.");
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<CustomizerDatabase>(path);
        }

        /// <summary>
        /// Explicitly creates the database asset next to this script, if one
        /// doesn't already exist anywhere in the project. Intended to be
        /// called only from the "Create Database Asset" menu item - not from
        /// any automatic/lazy code path.
        /// </summary>
        public static CustomizerDatabase CreateDatabaseAsset()
        {
            var existing = FindExisting();
            if (existing != null)
            {
                instance = existing;
                searchedForInstance = true;
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                Debug.Log($"[HierarchyCustomizer] A database asset already exists at " +
                          $"{AssetDatabase.GetAssetPath(existing)} - selecting it instead of creating a new one.");
                return existing;
            }

            var path = $"{PluginFolder()}/{AssetFileName}";
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var db = CreateInstance<CustomizerDatabase>();
            AssetDatabase.CreateAsset(db, path);
            AssetDatabase.SaveAssets();

            instance = db;
            searchedForInstance = true;

            Selection.activeObject = db;
            EditorGUIUtility.PingObject(db);
            Debug.Log($"[HierarchyCustomizer] Created database asset at {path}. " +
                      "Commit this file (and its .meta) to source control.");
            return db;
        }

        /// <summary>
        /// Resolves the folder this script itself lives in, used only as the
        /// default location for CreateDatabaseAsset(). CallerFilePath is
        /// filled in by the compiler with the path of the file that calls
        /// this method - since every call site is inside this same file,
        /// that's always CustomizerDatabase.cs's own location.
        /// </summary>
        private static string PluginFolder([CallerFilePath] string sourceFilePath = "")
        {
            if (cachedFolder != null) return cachedFolder;

            string sourceFullPath = sourceFilePath.Replace('\\', '/');
            string dataPath = Application.dataPath.Replace('\\', '/'); // ".../MyProject/Assets"
            string projectRoot = dataPath.Substring(0, dataPath.Length - "Assets".Length);

            string relative = sourceFullPath.StartsWith(projectRoot, StringComparison.OrdinalIgnoreCase)
                ? sourceFullPath.Substring(projectRoot.Length)
                : "Assets/Editor/HierarchyCustomizer/CustomizerDatabase.cs"; // fallback if path resolution fails

            cachedFolder = Path.GetDirectoryName(relative)?.Replace('\\', '/');
            if (string.IsNullOrEmpty(cachedFolder))
                cachedFolder = "Assets";

            return cachedFolder;
        }

        private void BuildLookup()
        {
            lookup = new Dictionary<string, CustomizerEntry>();
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.key) && !lookup.ContainsKey(e.key))
                    lookup.Add(e.key, e);
            }
        }

        public CustomizerEntry Get(string key)
        {
            if (string.IsNullOrEmpty(key)) return null;
            if (lookup == null) BuildLookup();
            lookup.TryGetValue(key, out var entry);
            return entry;
        }

        public CustomizerEntry GetOrCreate(string key)
        {
            var e = Get(key);
            if (e != null) return e;

            e = new CustomizerEntry { key = key };
            entries.Add(e);
            lookup[key] = e;
            return e;
        }

        public void Remove(string key)
        {
            if (lookup == null) BuildLookup();
            entries.RemoveAll(e => e.key == key);
            lookup.Remove(key);
            Save();
        }

        public void ClearAll()
        {
            entries.Clear();
            lookup?.Clear();
            Save();
        }

        public void Save()
        {
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
    }
}
