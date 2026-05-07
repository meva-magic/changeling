Shader "Custom/PSXTreeWind"
{
    Properties
    {
        _MainTex ("Tree Texture", 2D) = "white" {}
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
        
        [Header(Wind)]
        _WindStrength ("Wind Strength", Range(0, 0.5)) = 0.15
        _WindSpeed ("Wind Speed", Range(0, 3)) = 0.8
        _PivotHeight ("Pivot Height", Range(0, 1)) = 0.7
    }
    SubShader
    {
        Tags { 
            "RenderType"="TransparentCutout" 
            "Queue"="AlphaTest" 
            "RenderPipeline"="UniversalPipeline"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        Cull Off

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
            
            float _WindStrength;
            float _WindSpeed;
            float _PivotHeight;
            float _Cutoff;

            Varyings vert(Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 worldNormal = TransformObjectToWorldNormal(input.normalOS);
                
                float3 objectOrigin = TransformObjectToWorld(float3(0, 0, 0));
                float3 objectTop = TransformObjectToWorld(float3(0, 0.5, 0));
                float treeHeight = distance(objectOrigin, objectTop);
                
                float normalizedHeight = (worldPos.y - objectOrigin.y) / treeHeight;
                
                float time = _Time.y * _WindSpeed;
                float2 windDir = normalize(float2(0.7, 0.7));
                
                float noise = sin(objectOrigin.x * 1.7 + time) * cos(objectOrigin.z * 2.3 + time * 0.7);
                noise += sin(objectOrigin.x * 3.1 + time * 1.3) * cos(objectOrigin.z * 4.7 + time * 0.9) * 0.5;
                noise *= 0.5;
                
                float windAmount = noise * _WindStrength;
                
                float bendFactor = smoothstep(_PivotHeight, _PivotHeight + 0.1, normalizedHeight);
                float heightAbovePivot = max(0, normalizedHeight - _PivotHeight);
                bendFactor *= heightAbovePivot * 3.0;
                
                worldPos.x += windAmount * windDir.x * bendFactor;
                worldPos.z += windAmount * windDir.y * bendFactor;
                
                output.positionCS = TransformWorldToHClip(worldPos);
                output.positionWS = worldPos;
                output.normalWS = worldNormal;
                output.uv = input.uv;
                
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, input.uv);
                clip(texColor.a - _Cutoff);
                
                Light mainLight = GetMainLight();
                float NdotL = abs(dot(input.normalWS, mainLight.direction));
                half3 diffuse = mainLight.color * NdotL;
                half3 ambient = SampleSH(input.normalWS);
                half shadowAtten = MainLightRealtimeShadow(TransformWorldToShadowCoord(input.positionWS));
                half3 lighting = ambient + (diffuse * shadowAtten);
                
                texColor.rgb *= lighting;
                return texColor;
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
            float _WindStrength;
            float _WindSpeed;
            float _PivotHeight;
            float _Cutoff;

            Varyings ShadowVert(Attributes input)
            {
                Varyings output;
                
                float3 worldPos = TransformObjectToWorld(input.positionOS.xyz);
                float3 objectOrigin = TransformObjectToWorld(float3(0, 0, 0));
                float3 objectTop = TransformObjectToWorld(float3(0, 0.5, 0));
                float treeHeight = distance(objectOrigin, objectTop);
                float normalizedHeight = (worldPos.y - objectOrigin.y) / treeHeight;
                
                float time = _Time.y * _WindSpeed;
                float2 windDir = normalize(float2(0.7, 0.7));
                
                float noise = sin(objectOrigin.x * 1.7 + time) * cos(objectOrigin.z * 2.3 + time * 0.7);
                noise += sin(objectOrigin.x * 3.1 + time * 1.3) * cos(objectOrigin.z * 4.7 + time * 0.9) * 0.5;
                noise *= 0.5;
                
                float windAmount = noise * _WindStrength;
                float bendFactor = smoothstep(_PivotHeight, _PivotHeight + 0.1, normalizedHeight);
                float heightAbovePivot = max(0, normalizedHeight - _PivotHeight);
                bendFactor *= heightAbovePivot * 3.0;
                
                worldPos.x += windAmount * windDir.x * bendFactor;
                worldPos.z += windAmount * windDir.y * bendFactor;
                
                output.positionCS = TransformWorldToHClip(worldPos);
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