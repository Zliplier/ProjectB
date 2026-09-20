using UnityEditor;

namespace HierarchyCustomizer
{
    public static class HierarchyCustomizerMenu
    {
        [MenuItem("Tools/Hierarchy Customizer/Create Database Asset")]
        private static void CreateDatabase()
        {
            CustomizerDatabase.CreateDatabaseAsset();
        }

        [MenuItem("Tools/Hierarchy Customizer/Select Database Asset")]
        private static void SelectDatabase()
        {
            var db = CustomizerDatabase.Instance;
            if (db == null)
            {
                if (EditorUtility.DisplayDialog(
                    "No Database Asset Found",
                    "No CustomizerDatabase asset exists in this project yet. Create one now?",
                    "Create", "Cancel"))
                {
                    CustomizerDatabase.CreateDatabaseAsset();
                }
                return;
            }

            Selection.activeObject = db;
            EditorGUIUtility.PingObject(db);
        }

        [MenuItem("Tools/Hierarchy Customizer/Clear All Customizations")]
        private static void ClearAll()
        {
            var db = CustomizerDatabase.Instance;
            if (db == null)
            {
                EditorUtility.DisplayDialog(
                    "No Database Asset Found",
                    "There's no CustomizerDatabase asset to clear yet.",
                    "OK");
                return;
            }

            if (EditorUtility.DisplayDialog(
                "Clear All Customizations",
                "This removes every color and icon assigned in the Hierarchy and Project windows. This cannot be undone.",
                "Clear All", "Cancel"))
            {
                db.ClearAll();
                EditorApplication.RepaintHierarchyWindow();
                EditorApplication.RepaintProjectWindow();
            }
        }
    }
}
