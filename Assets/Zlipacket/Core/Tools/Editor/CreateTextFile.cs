// CreateTextFile.cs
// Place this script inside a folder named "Editor" anywhere under Assets/
// (e.g. Assets/Editor/CreateTextFile.cs).
//
// After it compiles, right-click in the Project window:
//   Create > Text File   -> new empty .txt asset, named like Unity's
//                            built-in "Create > C# Script" flow.

using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public static class CreateTextFile
{
    private const string DefaultFileName = "NewTextFile.txt";

    [MenuItem("Assets/Create/Text File", priority = 82)]
    public static void CreatePlainTextFile()
    {
        var action = ScriptableObject.CreateInstance<DoCreateTextAsset>();

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            action,
            DefaultFileName,
            GetTextIcon(),
            null);
    }

    private static Texture2D GetTextIcon()
    {
        var content = EditorGUIUtility.IconContent("TextAsset Icon");
        return content != null ? content.image as Texture2D : null;
    }

    // Handles the actual file write once the user confirms the filename.
    private class DoCreateTextAsset : EndNameEditAction
    {
        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string fileName = Path.GetFileName(pathName);

            // Keep it minimal — just a friendly header comment-ish first line.
            // Since .txt has no comment syntax, leave it blank by default,
            // or uncomment the line below if you'd like a title stamped in.
            string content = "";
            // string content = fileName + "\n";

            File.WriteAllText(pathName, content);
            AssetDatabase.ImportAsset(pathName);

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }
    }
}
