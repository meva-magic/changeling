Shader "Custom/DoubleSidedLit"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
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
            half4 _Color;
            half _Glossiness;
            half _Metallic;
            half _Cutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS);
                output.normalWS = normalInput.normalWS;
                
                output.uv = TRANSFORM_TEX(input.uv, _MainTex);
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                half4 col = texColor * _Color;
                
                clip(col.a - _Cutoff);
                
                Light mainLight = GetMainLight();
                
                float NdotL = abs(dot(input.normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 halfDir = normalize(mainLight.direction + viewDir);
                float specular = pow(max(0.0, abs(dot(input.normalWS, halfDir))), _Glossiness * 256);
                half3 specColor = mainLight.color * specular * _Metallic;
                
                half3 ambient = SampleSH(input.normalWS);
                half shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(input.positionWS));
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
                
                col.rgb *= lighting;
                
                return col;
            }
            ENDHLSL
        }
    }
    FallBack "Universal Render Pipeline/Simple Lit"
}