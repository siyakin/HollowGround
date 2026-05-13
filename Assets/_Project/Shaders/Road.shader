Shader "HollowGround/Road"
{
    Properties
    {
        _BaseMap       ("Dirt Texture",   2D)    = "white" {}
        _FreshColor    ("Fresh Color",    Color) = (0.45, 0.37, 0.27, 1)
        _WornColor     ("Worn Color",     Color) = (0.72, 0.63, 0.50, 1)
        _Wear          ("Wear",           Range(0, 1)) = 0
        _CenterBoost   ("Center Boost",   Range(0, 0.3)) = 0.18
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry+1"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "RoadForward"
            Tags { "LightMode" = "UniversalForward" }

            Cull [_Cull]
            ZWrite On

            HLSLPROGRAM
            #pragma vertex   RoadVert
            #pragma fragment RoadFrag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            // ── structs ─────────────────────────────────────────────────────

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float  centerMask : TEXCOORD2;
            };

            // ── uniforms ────────────────────────────────────────────────────

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4  _FreshColor;
                half4  _WornColor;
                half   _Wear;
                half   _CenterBoost;
            CBUFFER_END

            // ── vertex ──────────────────────────────────────────────────────

            Varyings RoadVert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv         = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS   = TransformObjectToWorldNormal(IN.normalOS);

                // Mask brightening toward center of tile (simulates foot traffic)
                float2 d = IN.uv - 0.5;
                OUT.centerMask = 1.0 - saturate(length(d) * 2.2);

                return OUT;
            }

            // ── fragment ────────────────────────────────────────────────────

            half4 RoadFrag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);

                // Blend fresh -> worn based on _Wear (set per-tile via MPB)
                half4 baseCol = lerp(_FreshColor, _WornColor, _Wear);

                // Worn tiles get brighter in the center (traffic path)
                baseCol.rgb += _CenterBoost * _Wear * IN.centerMask;

                half4 col = baseCol * tex;

                // Simple directional + ambient lighting
                Light mainLight = GetMainLight();
                half3 diffuse = saturate(dot(normalize(IN.normalWS), mainLight.direction))
                              * mainLight.color;
                col.rgb *= diffuse * 0.7 + 0.5;   // 0.5 = ambient floor

                return col;
            }
            ENDHLSL
        }

        // Shadow caster pass (required for _MAIN_LIGHT_SHADOWS)
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            Cull [_Cull]
            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            float3 _LightDirection;

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            Varyings ShadowVert(Attributes IN)
            {
                Varyings OUT;
                float3 posWS    = TransformObjectToWorld(IN.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionCS  = TransformWorldToHClip(
                    ApplyShadowBias(posWS, normalWS, _LightDirection));
                #if UNITY_REVERSED_Z
                    OUT.positionCS.z = min(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    OUT.positionCS.z = max(OUT.positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                return OUT;
            }

            half4 ShadowFrag(Varyings IN) : SV_Target { return 0; }
            ENDHLSL
        }
    }
}
