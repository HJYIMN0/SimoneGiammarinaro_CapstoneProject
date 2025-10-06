#ifndef CUSTOM_LIGHTING_INCLUDED
#define CUSTOM_LIGHTING_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

void GetLighting_float(float3 WorldPos, float3 WorldNormal, out float NdotL, out float Shadow)
{
#ifdef SHADERGRAPH_PREVIEW
    NdotL = 1.0;
    Shadow = 1.0;
#else
    float4 shadowCoord = TransformWorldToShadowCoord(WorldPos);
    Light mainLight = GetMainLight(shadowCoord);

    NdotL = saturate(dot(WorldNormal, mainLight.direction));
    Shadow = mainLight.shadowAttenuation;
#endif
}

#endif