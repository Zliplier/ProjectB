using UnityEditor;
using UnityEngine;
using System.Linq;
 
/// <summary>
/// Lets you set the tangent mode of any selected AnimationClip asset(s)
/// to Constant (stepped, no interpolation), Clamped Auto (Unity's normal
/// smooth curves), or Linear (straight-line segments) from the Project
/// window right-click menu.
///
/// Select one or more .anim clips (or clips inside an FBX) in the Project
/// window, then use:
///   Assets > Animation Tangents > Set Constant (No Interpolation)
///   Assets > Animation Tangents > Set Clamped Auto (Smooth)
///   Assets > Animation Tangents > Set Linear
/// </summary>

public class AnimationTangents : MonoBehaviour
{
    private const string ConstantMenuPath   = "Assets/Animation Tangents/Set Constant (No Interpolation)";
    private const string AutoMenuPath   = "Assets/Animation Tangents/Set Auto";
    private const string ClampedMenuPath    = "Assets/Animation Tangents/Set Clamped Auto";
    private const string LinearMenuPath     = "Assets/Animation Tangents/Set Linear";
    private const string FreeMenuPath     = "Assets/Animation Tangents/Set Free";
 
    [MenuItem(ConstantMenuPath)]
    private static void SetConstant()
    {
        ApplyTangentMode(AnimationUtility.TangentMode.Constant);
    }
 
    [MenuItem(ConstantMenuPath, true)]
    private static bool ValidateSetConstant() => HasClipSelected();
 
    [MenuItem(AutoMenuPath)]
    private static void SetAuto()
    {
        ApplyTangentMode(AnimationUtility.TangentMode.Auto);
    }
 
    [MenuItem(AutoMenuPath, true)]
    private static bool ValidateSetAuto() => HasClipSelected();
    
    [MenuItem(ClampedMenuPath)]
    private static void SetClampedAuto()
    {
        // Unity's default auto-smoothed tangents — curve bends to avoid
        // overshooting past neighboring keys.
        ApplyTangentMode(AnimationUtility.TangentMode.ClampedAuto);
    }
 
    [MenuItem(ClampedMenuPath, true)]
    private static bool ValidateSetClampedAuto() => HasClipSelected();
 
    [MenuItem(LinearMenuPath)]
    private static void SetLinear()
    {
        // Straight-line segments between keys — constant rate of change,
        // no easing in/out.
        ApplyTangentMode(AnimationUtility.TangentMode.Linear);
    }
 
    [MenuItem(LinearMenuPath, true)]
    private static bool ValidateSetLinear() => HasClipSelected();
 
    [MenuItem(FreeMenuPath)]
    private static void SetFree()
    {
        ApplyTangentMode(AnimationUtility.TangentMode.Free);
    }
 
    [MenuItem(FreeMenuPath, true)]
    private static bool ValidateSetFree() => HasClipSelected();

    
    private static bool HasClipSelected() => Selection.objects.Any(o => o is AnimationClip);
 
    private static void ApplyTangentMode(AnimationUtility.TangentMode mode)
    {
        int clipsChanged = 0;
 
        foreach (var obj in Selection.objects)
        {
            if (obj is not AnimationClip clip) continue;
 
            EditorCurveBinding[] bindings = AnimationUtility.GetCurveBindings(clip);
            AnimationCurve[] curves = bindings.Select(b => AnimationUtility.GetEditorCurve(clip, b)).ToArray();
 
            foreach (var curve in curves)
            {
                for (int j = 0; j < curve.keys.Length; j++)
                {
                    AnimationUtility.SetKeyRightTangentMode(curve, j, mode);
                    AnimationUtility.SetKeyLeftTangentMode(curve, j, mode);
                }
            }
 
            // Batch write — same reasoning as before: this is far faster than
            // calling SetEditorCurve per-binding in a loop.
            AnimationUtility.SetEditorCurves(clip, bindings, curves);
            EditorUtility.SetDirty(clip);
            clipsChanged++;
        }
 
        if (clipsChanged > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log($"[TangentModeTools] Set {clipsChanged} clip(s) to {mode} tangents.");
        }
    }
}
