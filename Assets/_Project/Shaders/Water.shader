Shader "HollowGround/Water"
{
    Properties
    {
        _BaseColor          ("Base Color",          Color)         = (0.04, 0.12, 0.30, 0.90)
        _ShallowColor       ("Shallow Color",       Color)         = (0.10, 0.50, 0.60, 0.60)
        _FoamColor          ("Foam Color",          Color)         = (0.85, 0.92, 0.95, 1.00)
        _DepthFactor        ("Depth Factor",        Range(0, 5))   = 2.0
        _WaveSpeed          ("Wave Speed",          Range(0, 2))   = 0.5
        _WaveHeight         ("Wave Height",         Range(0, 0.2)) = 0.05
        _FoamAmount         ("Foam Amount",         Range(0, 1))   = 0.28
        _FresnelPower       ("Fresnel Power",       Range(1, 5))   = 2.5
        _NormalStrength     ("Normal Strength",     Range(0, 1))   = 0.45
        _Opacity            ("Opacity",             Range(0, 1))   = 0.85
        _RefractionStrength ("Refraction Strength", Range(0, 0.1)) = 0.02
    }

    SubShader
    {
        Tags
        {
            "RenderType"      = "Transparent"
            "Queue"           = "Transparent+10"
            "RenderPipeline"  = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "UniversalForward"
            Tags { "LightMode" = "UniversalForward" }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex   WaterVert
            #pragma fragment WaterFrag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE _MAIN_LIGHT_SHADOWS_SCREEN
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"
            #if defined(_CameraOpaqueTexture_ENABLED)
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"
            #endif

            #define HG_TWO_PI 6.28318530718

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShallowColor;
                half4 _FoamColor;
                float _DepthFactor;
                float _WaveSpeed;
                float _WaveHeight;
                float _FoamAmount;
                float _FresnelPower;
                float _NormalStrength;
                float _Opacity;
                float _RefractionStrength;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float2 uv         : TEXCOORD2;
                float4 screenPos  : TEXCOORD3;
                float  waveDispY  : TEXCOORD4;
                float  fogCoord   : TEXCOORD5;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float Hash21(float2 p)
            {
                p  = frac(p * float2(127.1, 311.7));
                p += dot(p, p.yx + 19.19);
                return frac(p.x * p.y);
            }

            float ValueNoise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                float  a = Hash21(i);
                float  b = Hash21(i + float2(1, 0));
                float  c = Hash21(i + float2(0, 1));
                float  d = Hash21(i + float2(1, 1));
                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float FBM(float2 p, int octaves)
            {
                float val  = 0.0;
                float amp  = 0.5;
                float freq = 1.0;
                for (int i = 0; i < octaves; i++)
                {
                    val  += ValueNoise(p * freq) * amp;
                    freq *= 2.17;
                    amp  *= 0.48;
                }
                return val;
            }

            void ApplyGerstnerWave(
                float2 dir, float ampWeight, float wavelength, float steepness,
                float speedMult,
                float3 posWS, float time,
                inout float3 disp, inout float3 tangent, inout float3 binormal)
            {
                float2 d  = normalize(dir);
                float  k  = HG_TWO_PI / wavelength;
                float  c  = sqrt(9.8 / k) * speedMult;
                float  a  = ampWeight * _WaveHeight;
                float  q  = steepness;
                float  f  = k * dot(d, posWS.xz) - c * time;
                float  sf = sin(f);
                float  cf = cos(f);

                disp    += float3(q * a * d.x * cf,  a * sf, q * a * d.y * cf);
                tangent  += float3(-q * d.x * d.x * k * a * sf,  d.x * k * a * cf, -q * d.x * d.y * k * a * sf);
                binormal += float3(-q * d.x * d.y * k * a * sf,  d.y * k * a * cf, -q * d.y * d.y * k * a * sf);
            }

            float3 ProceduralNormal(float2 xz, float time)
            {
                float2 uv1 = xz * 0.6  + float2( time * 0.04,  time * 0.025);
                float2 uv2 = xz * 1.3  + float2(-time * 0.03, -time * 0.05);
                float2 uv3 = xz * 2.8  + float2( time * 0.08, -time * 0.06);

                float d = 0.08;

                float h   = FBM(uv1, 3) * 0.6 + FBM(uv2, 3) * 0.3 + FBM(uv3, 2) * 0.1;
                float hpx = FBM(uv1 + float2(d, 0), 3) * 0.6 + FBM(uv2 + float2(d, 0), 3) * 0.3 + FBM(uv3 + float2(d, 0), 2) * 0.1;
                float hpz = FBM(uv1 + float2(0, d), 3) * 0.6 + FBM(uv2 + float2(0, d), 3) * 0.3 + FBM(uv3 + float2(0, d), 2) * 0.1;

                float nx = -(hpx - h);
                float nz = -(hpz - h);

                return normalize(float3(nx, 0.15, nz));
            }

            Varyings WaterVert(Attributes input)
            {
                Varyings output = (Varyings)0;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 posWS   = TransformObjectToWorld(input.positionOS.xyz);
                float  time    = _Time.y * _WaveSpeed;

                float3 disp    = (float3)0;
                float3 tangent = float3(1, 0, 0);
                float3 binorm  = float3(0, 0, 1);

                ApplyGerstnerWave(float2( 0.71,  0.71), 0.40, 8.0,  0.14, 1.0,  posWS, time, disp, tangent, binorm);
                ApplyGerstnerWave(float2(-0.39,  0.92), 0.30, 5.5,  0.11, 0.85, posWS, time, disp, tangent, binorm);
                ApplyGerstnerWave(float2( 0.20, -0.98), 0.20, 3.5,  0.07, 1.15, posWS, time, disp, tangent, binorm);
                ApplyGerstnerWave(float2( 0.90,  0.44), 0.06, 1.8,  0.03, 1.4,  posWS, time, disp, tangent, binorm);
                ApplyGerstnerWave(float2(-0.56, -0.83), 0.04, 1.2,  0.02, 1.6,  posWS, time, disp, tangent, binorm);

                posWS += disp;

                output.positionWS = posWS;
                output.normalWS   = normalize(cross(binorm, tangent));
                output.uv         = input.uv;
                output.waveDispY  = disp.y;
                output.positionCS = TransformWorldToHClip(posWS);
                output.screenPos  = ComputeScreenPos(output.positionCS);
                output.fogCoord   = ComputeFogFactor(output.positionCS.z);

                return output;
            }

            half4 WaterFrag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                float2 screenUV   = input.screenPos.xy / input.screenPos.w;

                float  rawDepth   = SampleSceneDepth(screenUV);
                float  sceneDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float  depthDiff  = max(0.0, sceneDepth - input.screenPos.w);
                float  depthFade  = saturate(depthDiff * _DepthFactor);
                float  edgeMask   = smoothstep(0.0, 0.15, depthDiff);

                float3 procNorm   = ProceduralNormal(input.positionWS.xz, _Time.y);
                float3 geoNorm    = normalize(input.normalWS);
                float3 normalWS   = normalize(lerp(geoNorm, procNorm, _NormalStrength));

                float3 viewDir    = normalize(GetCameraPositionWS() - input.positionWS);
                float  NdotV      = saturate(dot(normalWS, viewDir));
                float  fresnel    = pow(1.0 - NdotV, _FresnelPower);

                float2 refrOff    = normalWS.xz * _RefractionStrength * saturate(depthDiff * 3.0);
                #if defined(_CameraOpaqueTexture_ENABLED)
                half3  refrColor  = SampleSceneColor(screenUV + refrOff);
                #else
                half3  refrColor  = half3(0.0, 0.0, 0.0);
                #endif

                half4  waterColor = lerp(_ShallowColor, _BaseColor, depthFade);
                waterColor.rgb    = lerp(refrColor, waterColor.rgb, saturate(depthFade * 0.7 + 0.15));

                float  t          = _Time.y * _WaveSpeed;
                float2 wp         = input.positionWS.xz;

                float  foamNoise  = FBM(wp * 1.4 + float2( t * 0.28,  t * 0.12), 4) * 0.60
                                  + FBM(wp * 3.2 + float2(-t * 0.18, -t * 0.32) + 7.3, 3) * 0.40;

                float  edgeFoam   = smoothstep(0.30, 0.0, depthDiff) * foamNoise;

                float  crestT     = saturate(input.waveDispY / max(_WaveHeight, 0.001));
                float  crestFoam  = smoothstep(0.65, 1.0, crestT) * foamNoise * 0.80;

                float  rippleFoam = FBM(wp * 2.8 + float2(t * 0.38, -t * 0.22), 3);
                float  patchyFoam = smoothstep(0.60, 0.82, rippleFoam) * 0.12;

                float  foam       = saturate((edgeFoam + crestFoam + patchyFoam) * _FoamAmount);

                waterColor.rgb    = lerp(waterColor.rgb, _FoamColor.rgb, foam);

                Light  mainLight  = GetMainLight();
                float3 halfDir    = normalize(viewDir + mainLight.direction);
                float  spec       = pow(saturate(dot(normalWS, halfDir)), 128.0) * fresnel;
                waterColor.rgb   += mainLight.color * spec * 0.70;

                float  edgeAlpha  = saturate(depthDiff * 6.0);
                float  baseAlpha  = lerp(_ShallowColor.a, _Opacity, depthFade);
                waterColor.a      = saturate(baseAlpha * edgeAlpha + fresnel * 0.15);

                waterColor.rgb    = MixFog(waterColor.rgb, input.fogCoord);

                return waterColor;
            }
            ENDHLSL
        }
    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
