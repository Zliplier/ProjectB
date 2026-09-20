using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// Tints Hierarchy rows and draws custom icons for GameObjects that have
    /// been assigned a color/icon, and shows a small button on hover (or
    /// always, once colored) to open the picker.
    /// </summary>
    [InitializeOnLoad]
    public static class HierarchyCustomizerHook
    {
        private const float ButtonSize = 16f;
        private const float IconSize = 16f;

        // Caches the (comparatively expensive) GlobalObjectId lookup per
        // instance ID so it isn't recomputed on every repaint. Invalidated
        // whenever the hierarchy structurally changes, and self-corrects if
        // an instance ID gets reused by a different object.
        private static readonly Dictionary<int, (GameObject go, string id)> globalIdCache =
            new Dictionary<int, (GameObject, string)>();

        private static GUIStyle whiteLabelStyle;
        private static GUIStyle blackLabelStyle;
        private static readonly GUIContent BulletContent = new GUIContent("\u25CF");

        static HierarchyCustomizerHook()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnGUI;
            EditorApplication.hierarchyChanged += () => globalIdCache.Clear();
        }

        private static void OnGUI(int instanceID, Rect selectionRect)
        {
            var db = CustomizerDatabase.Instance;
            if (db == null) return; // nothing created yet - tool is inactive until you create one

            var go = ResolveGameObject(instanceID);
            if (go == null) return;

            bool hovering = selectionRect.Contains(Event.current.mousePosition);

            string key = GetCachedGlobalId(instanceID, go);
            var entry = db.Get(key);
            bool hasColor = entry != null && entry.hasColor;
            bool hasIcon = entry != null && entry.hasIcon;

            // Nothing assigned and the mouse isn't over this row: skip all
            // remaining work, this is the common case for most rows.
            if (!hasColor && !hasIcon && !hovering) return;

            bool isRepaint = Event.current.type == EventType.Repaint;

            if (hasColor && isRepaint)
            {
                var tintRect = new Rect(0f, selectionRect.y, 4000f, selectionRect.height);
                EditorGUI.DrawRect(tintRect, entry.color);

                float labelX = selectionRect.x + (hasIcon ? IconSize + 2f : 0f);
                var labelRect = new Rect(labelX, selectionRect.y, selectionRect.width - (labelX - selectionRect.x), selectionRect.height);
                GUI.Label(labelRect, go.name, GetLabelStyle(entry.color));
            }

            if (hasIcon && isRepaint)
            {
                var tex = IconLibrary.Resolve(entry.iconName, entry.isCustomIcon);
                if (tex != null)
                {
                    var iconRect = new Rect(selectionRect.x, selectionRect.y, IconSize, IconSize);

                    // Without a color tint behind it, Unity's default icon is
                    // still sitting underneath ours - cover it first so the
                    // custom icon fully replaces it instead of both showing
                    // through each other's transparent padding.
                    if (!hasColor)
                    {
                        EditorGUI.DrawRect(iconRect, GetRowBackgroundColor(go));
                    }

                    GUI.DrawTexture(iconRect, tex, ScaleMode.ScaleToFit);
                }
            }

            var btnRect = new Rect(selectionRect.xMax - ButtonSize, selectionRect.y, ButtonSize, selectionRect.height);
            if (hovering || hasColor)
            {
                var prevColor = GUI.color;
                GUI.color = hasColor ? entry.color : new Color(1f, 1f, 1f, 0.5f);
                if (GUI.Button(btnRect, BulletContent, EditorStyles.label))
                {
                    PopupWindow.Show(btnRect, new IconColorPickerPopup(key, EditorApplication.RepaintHierarchyWindow));
                }
                GUI.color = prevColor;
            }
        }

        private static string GetCachedGlobalId(int instanceID, GameObject go)
        {
            if (globalIdCache.TryGetValue(instanceID, out var cached) && cached.go == go)
                return cached.id;

            var id = GlobalObjectId.GetGlobalObjectIdSlow(go).ToString();
            globalIdCache[instanceID] = (go, id);
            return id;
        }

        private static GameObject ResolveGameObject(int instanceID)
        {
            // EditorUtility.InstanceIDToObject was renamed to EntityIdToObject
            // in newer Unity versions. Use whichever exists for this Editor.
#if UNITY_6000_0_OR_NEWER
            return EditorUtility.EntityIdToObject(instanceID) as GameObject;
#else
            return EditorUtility.InstanceIDToObject(instanceID) as GameObject;
#endif
        }

        private static GUIStyle GetLabelStyle(Color bg)
        {
            float luminance = 0.299f * bg.r + 0.587f * bg.g + 0.114f * bg.b;
            bool useWhiteText = luminance <= 0.6f;

            if (useWhiteText)
            {
                if (whiteLabelStyle == null)
                    whiteLabelStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.white } };
                return whiteLabelStyle;
            }

            if (blackLabelStyle == null)
                blackLabelStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.black } };
            return blackLabelStyle;
        }

        // Approximates the row's actual background so we can cleanly occlude
        // the default icon underneath ours. Not pixel-perfect for every skin
        // state (e.g. an unfocused-window selection uses a slightly
        // different gray), but close enough that no ghosting shows through.
        private static Color GetRowBackgroundColor(GameObject go)
        {
            if (IsSelected(go))
                return new Color(0.24f, 0.48f, 0.90f);

            return EditorGUIUtility.isProSkin
                ? new Color(0.219f, 0.219f, 0.219f)
                : new Color(0.784f, 0.784f, 0.784f);
        }

        // Deliberately checks the actual GameObject reference via
        // Selection.gameObjects rather than an instance/entity ID. Unity is
        // mid-migration from int instance IDs to a new EntityId type across
        // the Editor API (Selection.Contains(int) included), and the exact
        // replacement APIs have been shifting between Unity 6.x point
        // releases. Since we already have the object reference here, this
        // sidesteps that churn entirely instead of chasing it.
        private static bool IsSelected(GameObject go)
        {
            var selected = Selection.gameObjects;
            for (int i = 0; i < selected.Length; i++)
            {
                if (selected[i] == go) return true;
            }
            return false;
        }
    }
}
