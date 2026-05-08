Shader "Custom/PaintingNoTile"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.1
        
        [Header(Scale Mode)]
        [Toggle(STRETCH)] _Stretch ("Stretch to Fill", Float) = 1
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma shader_feature STRETCH
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile_fragment _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float3 normalWS : TEXCOORD2;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            half4 _Color;
            float _Cutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                #ifndef STRETCH
                float2 textureSize = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)),
                    length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)),
                    1
                );
                
                float textureAspect = textureSize.x / textureSize.y;
                float quadAspect = objectScale.x / objectScale.y;
                
                if (quadAspect > textureAspect)
                {
                    float scale = textureAspect / quadAspect;
                    uv.x = (uv.x - 0.5) / scale + 0.5;
                }
                else
                {
                    float scale = quadAspect / textureAspect;
                    uv.y = (uv.y - 0.5) / scale + 0.5;
                }
                
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                {
                    discard;
                }
                #endif
                
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                half4 col = texColor * _Color;
                
                clip(col.a - _Cutoff);
                
                Light mainLight = GetMainLight();
                float NdotL = abs(dot(input.normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                half3 ambient = SampleSH(input.normalWS);
                half shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(input.positionWS));
                half3 lighting = ambient + (diffuse * shadowAtten);
                
                col.rgb *= lighting;
                return col;
            }
            ENDHLSL
        }
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            #pragma shader_feature STRETCH
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize;
            float _Cutoff;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.uv = input.uv;
                
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;
                
                #ifndef STRETCH
                float2 textureSize = float2(_MainTex_TexelSize.z, _MainTex_TexelSize.w);
                
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)),
                    length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)),
                    1
                );
                
                float textureAspect = textureSize.x / textureSize.y;
                float quadAspect = objectScale.x / objectScale.y;
                
                if (quadAspect > textureAspect)
                {
                    float scale = textureAspect / quadAspect;
                    uv.x = (uv.x - 0.5) / scale + 0.5;
                }
                else
                {
                    float scale = quadAspect / textureAspect;
                    uv.y = (uv.y - 0.5) / scale + 0.5;
                }
                
                if (uv.x < 0 || uv.x > 1 || uv.y < 0 || uv.y > 1)
                {
                    discard;
                }
                #endif
                
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}