// CreateHlslFile.cs
// Place this script inside a folder named "Editor" anywhere under Assets/
// (e.g. Assets/Editor/CreateHlslFile.cs). Unity Editor scripts must live
// in an "Editor" folder so they don't get compiled into player builds.
//
// After it compiles, right-click in the Project window:
//   Create > Shader > HLSL Include File          -> blank .hlsl with include guard
//   Create > Shader > HLSL Include File (URP)     -> .hlsl pre-wired with URP Core.hlsl include
//
// Both work exactly like Unity's built-in "Create > C# Script": you type
// a name, hit Enter, and the file is created with content matched to that name.

using System.IO;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;

public static class CreateHlslFile
{
    private const string DefaultFileName = "NewHLSLFile.hlsl";
    private const string DefaultUrpFileName = "NewHLSLFile_URP.hlsl";

    [MenuItem("Assets/Create/Shader/HLSL Include File", priority = 83)]
    public static void CreatePlainHlslFile()
    {
        var action = ScriptableObject.CreateInstance<DoCreateHlslAsset>();
        action.UseUrpTemplate = false;

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            action,
            DefaultFileName,
            GetShaderIcon(),
            null);
    }

    [MenuItem("Assets/Create/Shader/HLSL Include File (URP)", priority = 84)]
    public static void CreateUrpHlslFile()
    {
        var action = ScriptableObject.CreateInstance<DoCreateHlslAsset>();
        action.UseUrpTemplate = true;

        ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
            0,
            action,
            DefaultUrpFileName,
            GetShaderIcon(),
            null);
    }

    private static Texture2D GetShaderIcon()
    {
        // Falls back gracefully if the icon name isn't found in a given Editor skin/version.
        var content = EditorGUIUtility.IconContent("Shader Icon");
        return content != null ? content.image as Texture2D : null;
    }

    // Handles the actual file write once the user confirms the filename.
    private class DoCreateHlslAsset : EndNameEditAction
    {
        public bool UseUrpTemplate;

        public override void Action(int instanceId, string pathName, string resourceFile)
        {
            string fileName = Path.GetFileName(pathName);
            string guard = BuildIncludeGuard(pathName);

            string content = UseUrpTemplate
                ? BuildUrpTemplate(fileName, guard)
                : BuildPlainTemplate(fileName, guard);

            File.WriteAllText(pathName, content);
            AssetDatabase.ImportAsset(pathName);

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(pathName);
            ProjectWindowUtil.ShowCreatedAsset(asset);
        }

        private static string BuildIncludeGuard(string pathName)
        {
            string nameOnly = Path.GetFileNameWithoutExtension(pathName);
            // Sanitize into a valid, shout-case preprocessor token.
            var sb = new System.Text.StringBuilder(nameOnly.Length + 8);
            foreach (char c in nameOnly)
            {
                sb.Append(char.IsLetterOrDigit(c) ? char.ToUpperInvariant(c) : '_');
            }
            sb.Append("_INCLUDED");
            return sb.ToString();
        }

        private static string BuildPlainTemplate(string fileName, string guard)
        {
            return
$@"#ifndef {guard}
#define {guard}

// {fileName}
// Shared HLSL functions, structs, and variables go here.
// Include this file from your shader(s) with:
//   #include ""Path/To/{fileName}""

#endif // {guard}
";
        }

        private static string BuildUrpTemplate(string fileName, string guard)
        {
            return
$@"#ifndef {guard}
#define {guard}

// {fileName}
// URP-flavored HLSL include. Adjust the relative path below if your
// project structure differs from a default URP package layout.
#include ""Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl""

// ---------------------------------------------------------------------
// Structs
// ---------------------------------------------------------------------
struct Attributes
{{
    float4 positionOS : POSITION;
    float2 uv         : TEXCOORD0;
}};

struct Varyings
{{
    float4 positionHCS : SV_POSITION;
    float2 uv           : TEXCOORD0;
}};

// ---------------------------------------------------------------------
// Vertex / Fragment
// ---------------------------------------------------------------------
Varyings Vert(Attributes IN)
{{
    Varyings OUT;
    VertexPositionInputs positionInputs = GetVertexPositionInputs(IN.positionOS.xyz);
    OUT.positionHCS = positionInputs.positionCS;
    OUT.uv = IN.uv;
    return OUT;
}}

half4 Frag(Varyings IN) : SV_Target
{{
    return half4(1, 1, 1, 1);
}}

#endif // {guard}
";
        }
    }
}