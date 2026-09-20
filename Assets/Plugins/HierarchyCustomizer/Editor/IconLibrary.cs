using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// A curated palette of built-in Unity editor icons, plus support for
    /// user-supplied custom textures, used to populate the icon picker grid.
    /// Feel free to add or remove names from BuiltinIconNames to taste -
    /// any name that EditorGUIUtility.IconContent recognizes will work.
    /// </summary>
    public static class IconLibrary
    {
        public static readonly string[] BuiltinIconNames =
        {
            "Folder Icon",
            "GameObject Icon",
            "Prefab Icon",
            "cs Script Icon",
            "ScriptableObject Icon",
            "Camera Icon",
            "Light Icon",
            "AudioSource Icon",
            "AudioListener Icon",
            "Terrain Icon",
            "MeshRenderer Icon",
            "SkinnedMeshRenderer Icon",
            "BoxCollider Icon",
            "SphereCollider Icon",
            "CapsuleCollider Icon",
            "Rigidbody Icon",
            "Canvas Icon",
            "RectTransform Icon",
            "Text Icon",
            "Image Icon",
            "Button Icon",
            "EventSystem Icon",
            "NavMeshAgent Icon",
            "WindZone Icon",
            "ReflectionProbe Icon",
            "LightProbeGroup Icon",
            "ParticleSystem Icon",
            "Animation Icon",
            "AnimatorController Icon",
            "SpriteRenderer Icon",
            "Grid Icon",
            "Tilemap Icon",
            "VideoPlayer Icon",
            "WheelCollider Icon",
            "LineRenderer Icon",
            "TrailRenderer Icon",
            "Skybox Icon",
        };

        private static readonly Dictionary<string, Texture2D> builtinCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, Texture2D> customCache = new Dictionary<string, Texture2D>();
        private static readonly Dictionary<string, GUIContent> builtinContentCache = new Dictionary<string, GUIContent>();

        static IconLibrary()
        {
            // Custom icons are cached by GUID for the editor session; clear
            // if the project's asset structure changes so a re-imported or
            // reassigned texture doesn't get stuck showing a stale result.
            EditorApplication.projectChanged += ClearCaches;
        }

        public static void ClearCaches()
        {
            builtinCache.Clear();
            customCache.Clear();
            builtinContentCache.Clear();
        }

        public static Texture2D GetBuiltin(string iconName)
        {
            if (string.IsNullOrEmpty(iconName)) return null;

            if (builtinCache.TryGetValue(iconName, out var cached))
                return cached;

            Texture2D tex = null;
            var content = EditorGUIUtility.IconContent(iconName);
            if (content != null)
                tex = content.image as Texture2D;

            builtinCache[iconName] = tex;
            return tex;
        }

        /// <summary>
        /// A cached GUIContent wrapping a built-in icon's texture, for use in
        /// the picker grid - avoids allocating a new GUIContent every repaint
        /// while the popup is open and being redrawn on hover/mouse move.
        /// </summary>
        public static GUIContent GetBuiltinContent(string iconName)
        {
            if (builtinContentCache.TryGetValue(iconName, out var cached))
                return cached;

            var tex = GetBuiltin(iconName);
            var content = tex != null ? new GUIContent(tex) : new GUIContent("?");
            builtinContentCache[iconName] = content;
            return content;
        }

        public static Texture2D GetCustom(string textureGuid)
        {
            if (string.IsNullOrEmpty(textureGuid)) return null;

            if (customCache.TryGetValue(textureGuid, out var cached))
                return cached;

            var path = AssetDatabase.GUIDToAssetPath(textureGuid);
            var tex = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            customCache[textureGuid] = tex;
            return tex;
        }

        public static Texture2D Resolve(string iconName, bool isCustom)
        {
            return isCustom ? GetCustom(iconName) : GetBuiltin(iconName);
        }
    }
}
