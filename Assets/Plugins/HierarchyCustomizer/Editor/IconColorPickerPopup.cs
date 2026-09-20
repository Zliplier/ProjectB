using System;
using UnityEditor;
using UnityEngine;

namespace HierarchyCustomizer
{
    /// <summary>
    /// Small popup shown when the user clicks the customize button next to a
    /// Hierarchy or Project item: a row of preset colors (plus a custom color
    /// field) followed by a grid of icons (plus a custom texture field).
    /// </summary>
    public class IconColorPickerPopup : PopupWindowContent
    {
        private static readonly Color[] PresetColors =
        {
            new Color(0.80f, 0.24f, 0.24f), // red
            new Color(0.85f, 0.55f, 0.18f), // orange
            new Color(0.78f, 0.74f, 0.20f), // olive / yellow
            new Color(0.35f, 0.70f, 0.30f), // green
            new Color(0.20f, 0.65f, 0.60f), // teal
            new Color(0.25f, 0.45f, 0.85f), // blue
            new Color(0.55f, 0.35f, 0.80f), // purple
            new Color(0.85f, 0.30f, 0.60f), // pink
        };

        private const int Columns = 6;
        private const float SwatchSize = 26f;
        private const float IconCellSize = 28f;
        private const float Padding = 6f;

        private readonly string key;
        private readonly Action onChanged;

        private static GUIStyle centeredLabelStyle;
        private static readonly GUIContent ClearContent = new GUIContent("\u2715");

        public IconColorPickerPopup(string key, Action onChanged)
        {
            this.key = key;
            this.onChanged = onChanged;
        }

        public override Vector2 GetWindowSize()
        {
            int iconCount = IconLibrary.BuiltinIconNames.Length + 1; // +1 for the clear button
            int iconRows = Mathf.CeilToInt(iconCount / (float)Columns);

            float height = Padding * 4 + SwatchSize + (iconRows * IconCellSize) + 24f;
            float width = Mathf.Max(Columns * IconCellSize + Padding * 2, 260f);
            return new Vector2(width, height);
        }

        public override void OnGUI(Rect rect)
        {
            var db = CustomizerDatabase.Instance;
            if (db == null)
            {
                EditorGUILayout.HelpBox(
                    "Database asset not found. Create one via Tools > Hierarchy Customizer > Create Database Asset.",
                    MessageType.Warning);
                return;
            }

            var entry = db.GetOrCreate(key);

            GUILayout.Space(Padding);
            DrawColorRow(entry);
            GUILayout.Space(Padding);
            DrawIconGrid(entry);
        }

        private void DrawColorRow(CustomizerEntry entry)
        {
            EditorGUILayout.BeginHorizontal();

            if (DrawSwatchButton(null, entry.hasColor == false))
            {
                entry.hasColor = false;
                Commit();
            }

            foreach (var color in PresetColors)
            {
                bool selected = entry.hasColor && ColorsApproxEqual(entry.color, color);
                if (DrawSwatchButton(color, selected))
                {
                    entry.hasColor = true;
                    entry.color = color;
                    Commit();
                }
            }

            EditorGUI.BeginChangeCheck();
            var customColor = EditorGUILayout.ColorField(
                GUIContent.none,
                entry.hasColor ? entry.color : Color.white,
                false, true, false,
                GUILayout.Width(SwatchSize), GUILayout.Height(SwatchSize));
            if (EditorGUI.EndChangeCheck())
            {
                entry.hasColor = true;
                entry.color = customColor;
                Commit();
            }

            EditorGUILayout.EndHorizontal();
        }

        private bool DrawSwatchButton(Color? color, bool selected)
        {
            var r = GUILayoutUtility.GetRect(SwatchSize, SwatchSize, GUILayout.Width(SwatchSize));
            bool clicked = GUI.Button(r, GUIContent.none);

            if (color.HasValue)
            {
                EditorGUI.DrawRect(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), color.Value);
            }
            else
            {
                var prev = GUI.color;
                GUI.color = selected ? Color.white : new Color(1, 1, 1, 0.6f);
                GUI.Label(r, ClearContent, CenteredLabel());
                GUI.color = prev;
            }

            if (selected)
                EditorGUI.DrawRect(new Rect(r.x, r.yMax - 3, r.width, 3), Color.white);

            return clicked;
        }

        private void DrawIconGrid(CustomizerEntry entry)
        {
            int drawnInRow = 0;
            EditorGUILayout.BeginHorizontal();

            var clearRect = GUILayoutUtility.GetRect(IconCellSize, IconCellSize, GUILayout.Width(IconCellSize));
            if (GUI.Button(clearRect, GUIContent.none))
            {
                entry.hasIcon = false;
                Commit();
            }
            GUI.Label(clearRect, ClearContent, CenteredLabel());
            drawnInRow++;

            foreach (var iconName in IconLibrary.BuiltinIconNames)
            {
                if (drawnInRow >= Columns)
                {
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                    drawnInRow = 0;
                }

                var cellRect = GUILayoutUtility.GetRect(IconCellSize, IconCellSize, GUILayout.Width(IconCellSize));
                bool selected = entry.hasIcon && !entry.isCustomIcon && entry.iconName == iconName;

                if (selected)
                    EditorGUI.DrawRect(cellRect, new Color(0.24f, 0.48f, 0.90f, 0.5f));

                if (GUI.Button(cellRect, IconLibrary.GetBuiltinContent(iconName)))
                {
                    entry.hasIcon = true;
                    entry.isCustomIcon = false;
                    entry.iconName = iconName;
                    Commit();
                }

                drawnInRow++;
            }

            EditorGUILayout.EndHorizontal();

            GUILayout.Space(Padding);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Custom icon:", GUILayout.Width(80));
            var currentCustom = entry.isCustomIcon ? IconLibrary.GetCustom(entry.iconName) : null;
            EditorGUI.BeginChangeCheck();
            var newTex = (Texture2D)EditorGUILayout.ObjectField(currentCustom, typeof(Texture2D), false);
            if (EditorGUI.EndChangeCheck())
            {
                if (newTex != null)
                {
                    var path = AssetDatabase.GetAssetPath(newTex);
                    entry.hasIcon = true;
                    entry.isCustomIcon = true;
                    entry.iconName = AssetDatabase.AssetPathToGUID(path);
                }
                else
                {
                    entry.hasIcon = false;
                }
                Commit();
            }
            EditorGUILayout.EndHorizontal();
        }

        private static GUIStyle CenteredLabel()
        {
            if (centeredLabelStyle == null)
                centeredLabelStyle = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
            return centeredLabelStyle;
        }

        private static bool ColorsApproxEqual(Color a, Color b)
        {
            return Mathf.Abs(a.r - b.r) < 0.01f && Mathf.Abs(a.g - b.g) < 0.01f && Mathf.Abs(a.b - b.b) < 0.01f;
        }

        private void Commit()
        {
            var db = CustomizerDatabase.Instance;
            if (db == null) return;
            db.Save();
            onChanged?.Invoke();
        }
    }
}
