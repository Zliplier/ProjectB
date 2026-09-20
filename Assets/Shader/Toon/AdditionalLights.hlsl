#ifndef ADDITIONALLIGHTS_INCLUDED
#define ADDITIONALLIGHTS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

float3 CalculateAdditionalLight(Light l, float3 position, float3 normal)
{
    float diffuse = saturate(dot(normal, l.direction));
    diffuse *= l.distanceAttenuation * l.shadowAttenuation;
    return l.color * diffuse;
}

void AdditionalLights_float(float3 Position, float3 Normal, out float3 Color)
{
    #if defined(SHADERGRAPH_PREVIEW)
    Color = float3(0.5f, 0.5f, 0.5f);
    #else
    
    Color = float3(0.0f, 0.0f, 0.0f);
    
    int pixelLightCount = GetAdditionalLightsCount();
    for (int i = 0; i < pixelLightCount; i++)
    {
        Light light = GetAdditionalLight(i, Position, half4(1,1,1,1));
        Color += CalculateAdditionalLight(light, Position, Normal);
    }
    
    #endif
}

#endif // ADDITIONALLIGHTS_INCLUDED
