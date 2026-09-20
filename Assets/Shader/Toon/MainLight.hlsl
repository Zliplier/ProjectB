#ifndef MAINLIGHT_INCLUDED
#define MAINLIGHT_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void MainLight_float(float3 Position, float3 Normal, float3 View, out float3 Color, out float3 Direction, out float Attenuation)
{
    #if defined(SHADERGRAPH_PREVIEW)
    Color = float3(0.5f, 0.5f, 0.5f);
    #else
    //Calculate Shadow Coord
    #if SHADOWS_SCREEN
    float4 clipPos = TransformWorldToHClip(Position);
    float4 shadowCoord = ComputeScreenPos(clipPos);
    #else
    float4 shadowCoord = TransformWorldToShadowCoord(Position);
    #endif
    
    Normal = normalize(Normal);
    View = SafeNormalize(View);

    Light light = GetMainLight(shadowCoord);
    Color = light.color;
    Direction = light.direction;
    Attenuation = light.shadowAttenuation;
    #endif
}

#endif // MAINLIGHT_INCLUDED
