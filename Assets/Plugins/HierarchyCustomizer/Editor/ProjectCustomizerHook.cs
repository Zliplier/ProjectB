using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// Draws a color and/or icon badge for Project window folders that have
    /// been customized. All drawing here is scoped strictly to the folder
    /// icon's own bounding box (never the whole cell/label) since Project
    /// window grid cells don't composite a full-cell overlay cleanly. The
    /// color re-draws the actual folder icon texture tinted with GUI.color,
    /// so it keeps looking like a folder instead of becoming a flat
    /// rectangle. A custom icon is drawn as a small badge in the
    /// bottom-right corner.
    /// </summary>
    [InitializeOnLoad]
    public static class ProjectCustomizerHook
    {
        private const float PickerButtonSize = 14f;

        // Most items in the Project window are files, not folders. Rather
        // than asking AssetDatabase "is this a folder?" on every repaint for
        // every visible item, cache the answer per GUID and only recompute
        // when the project's asset structure actually changes.
        private static readonly Dictionary<string, bool> folderCache = new Dictionary<string, bool>();
        private static readonly GUIContent BulletContent = new GUIContent("\u25CF");

        static ProjectCustomizerHook()
        {
            EditorApplication.projectWindowItemOnGUI += OnGUI;
            EditorApplication.projectChanged += () => folderCache.Clear();
        }

        private static void OnGUI(string guid, Rect selectionRect)
        {
            var db = CustomizerDatabase.Instance;
            if (db == null) return; // nothing created yet - tool is inactive until you create one

            if (!IsFolder(guid))
                return;

            bool hovering = selectionRect.Contains(Event.current.mousePosition);

            var entry = db.Get(guid);
            bool hasColor = entry != null && entry.hasColor;
            bool hasIcon = entry != null && entry.hasIcon;

            // Nothing assigned and the mouse isn't over this item: skip all
            // remaining work, this is the common case for most folders.
            if (!hasColor && !hasIcon && !hovering) return;

            bool isGridView = selectionRect.height > selectionRect.width;
            bool isRepaint = Event.current.type == EventType.Repaint;

            // The folder glyph's own bounding box: a 16x16 icon on the left
            // in list view, or a square filling the cell width in grid view
            // (the remaining height below it is the label).
            Rect iconRect = isGridView
                ? new Rect(selectionRect.x, selectionRect.y, selectionRect.width, selectionRect.width)
                : new Rect(selectionRect.x, selectionRect.y + (selectionRect.height - 16f) / 2f, 16f, 16f);

            if (hasColor && isRepaint)
            {
                DrawTintedFolderIcon(iconRect, entry.color);
            }

            if (hasIcon && isRepaint)
            {
                var tex = IconLibrary.Resolve(entry.iconName, entry.isCustomIcon);
                if (tex != null)
                {
                    float badgeSize = Mathf.Max(iconRect.width * 0.46f, 9f);
                    var badgeRect = new Rect(iconRect.xMax - badgeSize, iconRect.yMax - badgeSize, badgeSize, badgeSize);
                    GUI.DrawTexture(badgeRect, tex, ScaleMode.ScaleToFit);
                }
            }

            Rect btnRect = isGridView
                ? new Rect(iconRect.xMax - PickerButtonSize, iconRect.y, PickerButtonSize, PickerButtonSize)
                : new Rect(selectionRect.xMax - PickerButtonSize, selectionRect.y, PickerButtonSize, selectionRect.height);

            if (hovering || hasColor)
            {
                var prevColor = GUI.color;
                GUI.color = hasColor ? entry.color : new Color(1f, 1f, 1f, 0.6f);
                if (GUI.Button(btnRect, BulletContent, EditorStyles.label))
                {
                    PopupWindow.Show(btnRect, new IconColorPickerPopup(guid, EditorApplication.RepaintProjectWindow));
                }
                GUI.color = prevColor;
            }
        }

        private static bool IsFolder(string guid)
        {
            if (folderCache.TryGetValue(guid, out var cached))
                return cached;

            string path = AssetDatabase.GUIDToAssetPath(guid);
            bool isFolder = !string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path);
            folderCache[guid] = isFolder;
            return isFolder;
        }

        private static void DrawTintedFolderIcon(Rect iconRect, Color tint)
        {
            var folderTex = IconLibrary.GetBuiltin("Folder Icon");
            if (folderTex == null)
            {
                // Fallback if the built-in icon name isn't found on this
                // Unity version - still shows the color, just as a flat swatch.
                EditorGUI.DrawRect(iconRect, tint);
                return;
            }

            var prevColor = GUI.color;
            GUI.color = tint;
            GUI.DrawTexture(iconRect, folderTex, ScaleMode.ScaleToFit);
            GUI.color = prevColor;
        }
    }
}
