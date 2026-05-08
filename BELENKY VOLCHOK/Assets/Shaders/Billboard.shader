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
            "IgnoreProjector"="True"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
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
                
                // Get world position of object pivot (ignore object rotation)
                float4 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                
                // Get direction from object to camera, flat on XZ plane
                float3 toCamera = _WorldSpaceCameraPos - worldPos.xyz;
                float3 toCameraXZ = normalize(float3(toCamera.x, 0, toCamera.z));
                
                // Calculate right vector (perpendicular to camera direction, horizontal)
                float3 camRight = normalize(cross(float3(0, 1, 0), toCameraXZ));
                
                // Extract scale from object matrix
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float scaleY = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));
                float scaleZ = length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));
                
                // Get sign of X scale for flipping
                float3 objRight = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
                float signX = dot(normalize(objRight), float3(1, 0, 0)) < 0 ? -1 : 1;
                
                // Build vertex in local space with scale applied
                float3 localPos = float3(
                    input.positionOS.x * scaleX * signX,
                    input.positionOS.y * scaleY,
                    input.positionOS.z * scaleZ
                );
                
                // Billboard: rotate XZ around Y, keep Y as-is
                float3 worldVertex;
                worldVertex.x = worldPos.x + localPos.x * camRight.x + localPos.z * toCameraXZ.x;
                worldVertex.y = worldPos.y + localPos.y;
                worldVertex.z = worldPos.z + localPos.x * camRight.z + localPos.z * toCameraXZ.z;
                
                output.positionCS = TransformWorldToHClip(worldVertex);
                output.positionWS = worldVertex;
                output.normalWS = toCameraXZ;
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
                
                float4 worldPos = mul(unity_ObjectToWorld, float4(0, 0, 0, 1));
                
                float3 toCamera = _WorldSpaceCameraPos - worldPos.xyz;
                float3 toCameraXZ = normalize(float3(toCamera.x, 0, toCamera.z));
                float3 camRight = normalize(cross(float3(0, 1, 0), toCameraXZ));
                
                float scaleX = length(float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20));
                float scaleY = length(float3(unity_ObjectToWorld._m01, unity_ObjectToWorld._m11, unity_ObjectToWorld._m21));
                float scaleZ = length(float3(unity_ObjectToWorld._m02, unity_ObjectToWorld._m12, unity_ObjectToWorld._m22));
                
                float3 objRight = float3(unity_ObjectToWorld._m00, unity_ObjectToWorld._m10, unity_ObjectToWorld._m20);
                float signX = dot(normalize(objRight), float3(1, 0, 0)) < 0 ? -1 : 1;
                
                float3 localPos = float3(
                    input.positionOS.x * scaleX * signX,
                    input.positionOS.y * scaleY,
                    input.positionOS.z * scaleZ
                );
                
                float3 worldVertex;
                worldVertex.x = worldPos.x + localPos.x * camRight.x + localPos.z * toCameraXZ.x;
                worldVertex.y = worldPos.y + localPos.y;
                worldVertex.z = worldPos.z + localPos.x * camRight.z + localPos.z * toCameraXZ.z;
                
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