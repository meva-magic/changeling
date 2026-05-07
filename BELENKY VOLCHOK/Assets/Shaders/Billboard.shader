Shader "Custom/StandardBillboardLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        _Brightness ("Brightness", Range(0.5, 3)) = 1.0
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "Queue"="Transparent" 
            "RenderPipeline"="UniversalPipeline"
            "IgnoreProjector"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHT_SHADOWS
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
                float4 shadowCoord : TEXCOORD3;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;
            half4 _Color;
            half _Glossiness;
            half _Metallic;
            half _Cutoff;
            half _Brightness;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                // Get camera vectors
                float3 camRight = float3(UNITY_MATRIX_V._m00, UNITY_MATRIX_V._m10, UNITY_MATRIX_V._m20);
                float3 camUp = float3(UNITY_MATRIX_V._m01, UNITY_MATRIX_V._m11, UNITY_MATRIX_V._m21);
                float3 camForward = float3(UNITY_MATRIX_V._m02, UNITY_MATRIX_V._m12, UNITY_MATRIX_V._m22);
                
                // For Y-axis only billboarding: use camera right but keep world up
                float3 worldUp = float3(0, 1, 0);
                
                // Project camera right onto horizontal plane
                float3 horizontalRight = camRight;
                horizontalRight.y = 0;
                
                // If camera is directly above, fallback
                if (length(horizontalRight) < 0.001)
                {
                    horizontalRight = float3(1, 0, 0);
                }
                horizontalRight = normalize(horizontalRight);
                
                float3 objectScale = float3(
                    length(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20)),
                    length(float3(UNITY_MATRIX_M._m01, UNITY_MATRIX_M._m11, UNITY_MATRIX_M._m21)),
                    length(float3(UNITY_MATRIX_M._m02, UNITY_MATRIX_M._m12, UNITY_MATRIX_M._m22))
                );
                
                float3 scaledVertex = float3(
                    input.positionOS.x * objectScale.x, 
                    input.positionOS.y * objectScale.y, 
                    0
                );
                
                float flipFactor = 1.0;
                float3 worldRight = normalize(float3(UNITY_MATRIX_M._m00, UNITY_MATRIX_M._m10, UNITY_MATRIX_M._m20));
                float dotProduct = dot(worldRight, float3(1, 0, 0));
                
                if (dotProduct < -0.5)
                {
                    flipFactor = -1.0;
                }
                
                scaledVertex.x *= flipFactor;
                
                // Y-axis only billboarding
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * horizontalRight;  // Rotate around Y only
                worldVertex += scaledVertex.y * worldUp;           // Keep vertical axis fixed
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.positionWS = worldVertex;
                
                // Use camera forward projected to horizontal for normal
                float3 horizontalForward = camForward;
                horizontalForward.y = 0;
                if (length(horizontalForward) < 0.001)
                {
                    horizontalForward = float3(0, 0, 1);
                }
                horizontalForward = normalize(horizontalForward);
                output.normalWS = -horizontalForward;
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                output.shadowCoord = TransformWorldToShadowCoord(worldVertex);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = texColor * _Color;
                
                clip(col.a - _Cutoff);
                
                Light mainLight = GetMainLight(input.shadowCoord);
                
                float NdotL = abs(dot(input.normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float specular = pow(max(0.0, abs(dot(input.normalWS, halfDir))), _Glossiness * 256);
                half3 specColor = mainLight.color * specular * _Metallic;
                
                half3 ambient = SampleSH(input.normalWS);
                ambient = max(ambient, half3(0.1, 0.1, 0.1));
                
                half shadowAtten = mainLight.shadowAttenuation;
                half3 lighting = ambient + (diffuse * shadowAtten) + (specColor * shadowAtten);
                
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightsCount = GetAdditionalLightsCount();
                
                for (uint lightIndex = 0; lightIndex < additionalLightsCount; lightIndex++)
                {
                    Light additionalLight = GetAdditionalLight(lightIndex, input.positionWS);
                    half3 attenuatedLightColor = additionalLight.color * additionalLight.distanceAttenuation * additionalLight.shadowAttenuation;
                    
                    float addNdotL = abs(dot(input.normalWS, additionalLight.direction));
                    lighting += attenuatedLightColor * addNdotL;
                    
                    float3 addHalfDir = normalize(additionalLight.direction + viewDir);
                    float addSpecular = pow(max(0.0, abs(dot(input.normalWS, addHalfDir))), _Glossiness * 256);
                    lighting += attenuatedLightColor * addSpecular * _Metallic;
                }
                #endif
                
                lighting *= _Brightness;
                col.rgb *= lighting;
                
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}