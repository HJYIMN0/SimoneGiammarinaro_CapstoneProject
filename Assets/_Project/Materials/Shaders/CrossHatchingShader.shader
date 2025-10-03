Shader "Custom/CrossHatchingLit"
{
    Properties
    {
        _LightColor ("Light Color", Color) = (1,1,1,1)
        _PenumbraColor ("Penumbra Color", Color) = (0.6,0.6,0.6,1)
        _ShadowColor ("Shadow Color", Color) = (0.3,0.3,0.3,1)
        _HatchTex ("Hatch Texture", 2D) = "white" {}
        _HatchScale("Hatch Scale", Float) = 10.0
        [Header(Light Thresholds)]
        _ShadowThreshold("Shadow Threshold", Range(0,1)) = 0.3
        _PenumbraThreshold("Penumbra Threshold", Range(0,1)) = 0.7
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200
        Pass
        {
            Tags { "LightMode"="UniversalForward" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float fogCoord    : TEXCOORD3;
            };
            
            CBUFFER_START(UnityPerMaterial)
                float4 _LightColor;
                float4 _PenumbraColor;
                float4 _ShadowColor;
                float _HatchScale;
                float _ShadowThreshold;
                float _PenumbraThreshold;
            CBUFFER_END
            
            TEXTURE2D(_HatchTex);
            SAMPLER(sampler_HatchTex);
            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = IN.uv * _HatchScale;
                OUT.positionCS = TransformWorldToHClip(OUT.positionWS);
                OUT.fogCoord = ComputeFogFactor(OUT.positionCS.z);
                return OUT;
            }
            
            half4 frag(Varyings IN) : SV_Target
            {
                // IMPORTANTE: calcola shadowCoord QUI nel fragment shader
                // usando la posizione world-space interpolata
                float4 shadowCoord = TransformWorldToShadowCoord(IN.positionWS);
                
                // ottieni la luce principale CON le shadow coord corrette
                Light mainLight = GetMainLight(shadowCoord);
                
                // normalizza la normale (può perdere normalizzazione durante interpolazione)
                float3 normalWS = normalize(IN.normalWS);
                
                // calcola illuminazione
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                
                // ombre (mainLight.shadowAttenuation già contiene il valore corretto)
                float shadow = mainLight.shadowAttenuation;
                
                // combinazione luce + ombre
                float lightTerm = NdotL * shadow;
                
                // campiona texture hatch
                float hatch = SAMPLE_TEXTURE2D(_HatchTex, sampler_HatchTex, IN.uv).r;
                
                // Tre zone distinte basate su soglie
                float3 finalColor;
                
                if (lightTerm < _ShadowThreshold)
                {
                    // OMBRA: crosshatch + colore ombra
                    finalColor = _ShadowColor.rgb * hatch;
                }
                else if (lightTerm < _PenumbraThreshold)
                {
                    // PENOMBRA: colore grigio
                    finalColor = _PenumbraColor.rgb;
                }
                else
                {
                    // LUCE DIRETTA: colore chiaro
                    finalColor = _LightColor.rgb;
                }
                
                // applica fog se necessario
                finalColor = MixFog(finalColor, IN.fogCoord);
                
                return float4(finalColor, 1.0);
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/LitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}