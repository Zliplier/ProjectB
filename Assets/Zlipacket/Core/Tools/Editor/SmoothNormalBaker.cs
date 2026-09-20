// SmoothNormalBaker.cs
//
// Bakes "smooth normals" (normals averaged across all vertices that share the
// same POSITION, regardless of hard edges / UV seams / split normals) into a
// spare UV channel of the mesh. Use these baked normals in an inverted-hull
// outline shader instead of the real vertex normal, and the outline stops
// cracking open at smoothing-group / UV-seam boundaries.
//
// USAGE
//   Window > Tools > Smooth Normal Outline Baker
//   1. Select GameObject(s) in the scene (or select mesh assets in the
//      Project window) that have a MeshFilter or SkinnedMeshRenderer.
//   2. Pick the destination UV channel (default: UV8 / TEXCOORD7).
//   3. Click "Bake Smooth Normals". A new mesh asset "<MeshName>_SmoothUV.asset"
//      is created next to the original (or in Assets/ if the mesh has no
//      asset path) and assigned back onto the selected renderers.
//
// The baked data is written as a Vector3 (xyz = object-space smoothed
// normal) via Mesh.SetUVs, so it survives as TEXCOORD7 (or whichever
// channel you pick) in the vertex shader.

using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SmoothNormalBaker : EditorWindow
{
    // UV channel index as Unity's SetUVs expects it: 0 = uv, 1 = uv2, ... 7 = uv8
    private int uvChannelIndex = 7; // UV8 / TEXCOORD7 by default
    private float positionWeldThreshold = 0.0001f;
    private bool overwriteExistingUV = true;

    [MenuItem("Tools/Smooth Normal Outline Baker")]
    private static void Open()
    {
        GetWindow<SmoothNormalBaker>("Smooth Normal Baker");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Averages normals across vertices that share the same position, " +
            "then bakes the result into a UV channel for crack-free inverted-hull outlines.",
            MessageType.Info);

        EditorGUILayout.Space();

        string[] uvNames = { "UV1 (uv)", "UV2", "UV3", "UV4", "UV5", "UV6", "UV7", "UV8 (TEXCOORD7)" };
        uvChannelIndex = EditorGUILayout.Popup("Destination UV Channel", uvChannelIndex, uvNames);

        positionWeldThreshold = EditorGUILayout.FloatField(
            new GUIContent("Weld Distance", "Vertices closer than this are treated as the same point."),
            positionWeldThreshold);

        overwriteExistingUV = EditorGUILayout.Toggle(
            new GUIContent("Overwrite Existing UV Data", "If off and the channel already has data, baking is skipped for that mesh."),
            overwriteExistingUV);

        EditorGUILayout.Space();

        using (new EditorGUI.DisabledScope(Selection.gameObjects.Length == 0))
        {
            if (GUILayout.Button("Bake Smooth Normals For Selection", GUILayout.Height(30)))
            {
                BakeForSelection();
            }
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField($"Selected objects: {Selection.gameObjects.Length}");
    }

    private void BakeForSelection()
    {
        int processed = 0;

        foreach (GameObject go in Selection.gameObjects)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            SkinnedMeshRenderer smr = go.GetComponent<SkinnedMeshRenderer>();

            Mesh sourceMesh = mf != null ? mf.sharedMesh : (smr != null ? smr.sharedMesh : null);
            if (sourceMesh == null)
            {
                continue;
            }

            if (!overwriteExistingUV && HasUVData(sourceMesh, uvChannelIndex))
            {
                Debug.LogWarning($"[SmoothNormalBaker] Skipping '{sourceMesh.name}': UV channel {uvChannelIndex + 1} already has data.");
                continue;
            }

            Mesh baked = BakeMesh(sourceMesh, uvChannelIndex, positionWeldThreshold);
            string savedPath = SaveMeshAsset(baked, sourceMesh);

            if (mf != null) mf.sharedMesh = baked;
            if (smr != null) smr.sharedMesh = baked;

            Debug.Log($"[SmoothNormalBaker] Baked '{sourceMesh.name}' -> {savedPath}");
            processed++;
        }

        if (processed == 0)
        {
            Debug.LogWarning("[SmoothNormalBaker] No valid MeshFilter/SkinnedMeshRenderer found in selection.");
        }
        else
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }

    private static bool HasUVData(Mesh mesh, int channelIndex)
    {
        var list = new List<Vector3>();
        mesh.GetUVs(channelIndex, list);
        return list.Count > 0;
    }

    /// <summary>
    /// Returns a duplicated mesh with smoothed (position-welded, averaged)
    /// normals written into the given UV channel.
    /// </summary>
    public static Mesh BakeMesh(Mesh source, int uvChannelIndex, float weldThreshold)
    {
        Mesh mesh = Object.Instantiate(source);
        mesh.name = source.name + "_SmoothUV";

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        if (normals == null || normals.Length != vertices.Length)
        {
            mesh.RecalculateNormals();
            normals = mesh.normals;
        }

        // Group vertex indices by welded position.
        var positionGroups = new Dictionary<Vector3Int, List<int>> ();
        float inv = 1f / Mathf.Max(weldThreshold, 1e-6f);

        Vector3Int Key(Vector3 p) => new Vector3Int(
            Mathf.RoundToInt(p.x * inv),
            Mathf.RoundToInt(p.y * inv),
            Mathf.RoundToInt(p.z * inv));

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3Int key = Key(vertices[i]);
            if (!positionGroups.TryGetValue(key, out var list))
            {
                list = new List<int>();
                positionGroups[key] = list;
            }
            list.Add(i);
        }

        // Average normals within each position group.
        Vector3[] smoothNormals = new Vector3[vertices.Length];
        foreach (var kvp in positionGroups)
        {
            List<int> indices = kvp.Value;
            Vector3 sum = Vector3.zero;
            for (int i = 0; i < indices.Count; i++)
            {
                sum += normals[indices[i]];
            }
            Vector3 avg = sum.normalized;
            if (avg.sqrMagnitude < 1e-8f)
            {
                // Degenerate (normals cancelled out); fall back to first normal.
                avg = normals[indices[0]];
            }
            for (int i = 0; i < indices.Count; i++)
            {
                smoothNormals[indices[i]] = avg;
            }
        }

        var uvData = new List<Vector3>(smoothNormals.Length);
        uvData.AddRange(smoothNormals);
        mesh.SetUVs(uvChannelIndex, uvData);

        return mesh;
    }

    private static string SaveMeshAsset(Mesh baked, Mesh source)
    {
        string sourcePath = AssetDatabase.GetAssetPath(source);
        string directory = string.IsNullOrEmpty(sourcePath) ? "Assets" : Path.GetDirectoryName(sourcePath);
        string fileName = baked.name + ".asset";
        string fullPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, fileName).Replace("\\", "/"));

        AssetDatabase.CreateAsset(baked, fullPath);
        return fullPath;
    }
}