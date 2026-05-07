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
            "RenderType"="TransparentCutout" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline"
        }
        Cull Off
        LOD 200

        Pass
        {
            Name "DepthOnly"
            Tags { "LightMode"="DepthOnly" }
            
            ZWrite On
            ColorMask 0
            
            HLSLPROGRAM
            #pragma vertex DepthVert
            #pragma fragment DepthFrag
            
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
            half _Cutoff;

            Varyings DepthVert(Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                // Get camera direction in XZ plane only (ignore Y tilt)
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 cameraDirXZ = normalize(float3(cameraPos.x - worldPos.x, 0, cameraPos.z - worldPos.z));
                float3 camRight = normalize(cross(float3(0, 1, 0), cameraDirXZ));
                
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
                    flipFactor = -1.0;
                
                scaledVertex.x *= flipFactor;
                
                // Billboard on XZ plane, keep vertical orientation
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex.y += scaledVertex.y * objectScale.y;
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.uv = input.uv;
                
                return output;
            }

            half4 DepthFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha
            
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
                
                // Get camera direction in XZ plane only (ignore Y tilt)
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 cameraDirXZ = normalize(float3(cameraPos.x - worldPos.x, 0, cameraPos.z - worldPos.z));
                float3 camRight = normalize(cross(float3(0, 1, 0), cameraDirXZ));
                float3 camForward = cameraDirXZ;
                
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
                    flipFactor = -1.0;
                
                scaledVertex.x *= flipFactor;
                
                // Billboard on XZ plane, keep vertical orientation
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex.y += scaledVertex.y * objectScale.y;
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.positionWS = worldVertex;
                output.normalWS = camForward;
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
        
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            
            ZWrite On
            ZTest LEqual
            
            HLSLPROGRAM
            #pragma vertex ShadowVert
            #pragma fragment ShadowFrag
            
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
            half _Cutoff;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(float3(0, 0, 0));
                
                float3 cameraPos = _WorldSpaceCameraPos;
                float3 cameraDirXZ = normalize(float3(cameraPos.x - worldPos.x, 0, cameraPos.z - worldPos.z));
                float3 camRight = normalize(cross(float3(0, 1, 0), cameraDirXZ));
                
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
                    flipFactor = -1.0;
                
                scaledVertex.x *= flipFactor;
                
                float3 worldVertex = worldPos;
                worldVertex += scaledVertex.x * camRight;
                worldVertex.y += scaledVertex.y * objectScale.y;
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.uv = input.uv;
                
                return output;
            }

            half4 ShadowFrag(Varyings input) : SV_Target
            {
                half alpha = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv).a;
                clip(alpha - _Cutoff);
                return 0;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}