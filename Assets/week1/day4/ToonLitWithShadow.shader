Shader "Custom/ToonLitWithShadow"
{
    Properties
    {
        [MainTexture] _BaseMap("Albedo", 2D) = "white" {}
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        
        _Steps("Color Steps", Range(1, 10)) = 3
        _RampOffset("Ramp Offset", Range(-1, 1)) = 0
        
        _SpecularColor("Specular Color", Color) = (0.5, 0.5, 0.5, 1)
        _Smoothness("Smoothness", Range(0.1, 256)) = 32
        
        _OutlineWidth("Outline Width", Range(0, 0.1)) = 0.03
        _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType" = "Opaque" 
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }
        
        // 轮廓Pass（可选，进阶功能）
        Pass
        {
            Name "Outline"
            Cull Front
            
            HLSLPROGRAM
            #pragma vertex OutlineVertex
            #pragma fragment OutlineFragment
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            
            float _OutlineWidth;
            float4 _OutlineColor;
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };
            
            Varyings OutlineVertex(Attributes input)
            {
                Varyings output;
                
                // 沿法线方向扩展顶点
                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                positionWS += normalWS * _OutlineWidth;
                
                output.positionCS = TransformWorldToHClip(positionWS);
                return output;
            }
            
            half4 OutlineFragment(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }
            
            ENDHLSL
        }
        
        // 主光照Pass
        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            // 阴影相关宏
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fog
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 texcoord : TEXCOORD0;
            };
            
            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoord : TEXCOORD3;
                
                #if defined(_MAIN_LIGHT_SHADOWS)
                    float4 shadowCoord : TEXCOORD4;
                #endif
            };
            
            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Steps;
                float _RampOffset;
                float4 _SpecularColor;
                float _Smoothness;
            CBUFFER_END
            
            // 卡通漫反射计算函数
            float3 ToonDiffuse(float3 normal, float3 lightDir, float3 lightColor, float steps, float rampOffset)
            {
                float NdotL = dot(normal, lightDir);
                NdotL = max(0, NdotL);
                
                // 添加ramp偏移，控制阴影区域
                NdotL += rampOffset;
                NdotL = saturate(NdotL);
                
                // 色阶化处理
                float toonValue = floor(NdotL * steps) / steps;
                
                return lightColor * toonValue;
            }
            
            // Blinn-Phong高光计算函数
            float3 BlinnPhongSpecular(float3 normal, float3 lightDir, float3 viewDir, float3 lightColor, float smoothness)
            {
                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = saturate(dot(normal, halfDir));
                
                float specularTerm = pow(NdotH, smoothness);
                return lightColor * _SpecularColor.rgb * specularTerm;
            }
            
            Varyings vert(Attributes input)
            {
                Varyings output;
                
                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.texcoord, _BaseMap);
                
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                
                #if defined(_MAIN_LIGHT_SHADOWS)
                    output.shadowCoord = GetShadowCoord(vertexInput);
                #endif
                
                return output;
            }
            
            half4 frag(Varyings input) : SV_Target
            {
                // 基础纹理采样
                half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
                baseColor *= _BaseColor;
                
                // 法线归一化
                float3 normalWS = normalize(input.normalWS);
                
                // 获取主光源信息
                Light mainLight = GetMainLight();
                
                #if defined(_MAIN_LIGHT_SHADOWS)
                    mainLight.shadowAttenuation = MainLightRealtimeShadow(input.shadowCoord);
                #endif
                
                // 计算卡通漫反射
                float3 diffuse = ToonDiffuse(normalWS, mainLight.direction, mainLight.color, _Steps, _RampOffset);
                
                // 计算高光反射
                float3 viewDir = GetWorldSpaceNormalizeViewDir(input.positionWS);
                float3 specular = BlinnPhongSpecular(normalWS, mainLight.direction, viewDir, mainLight.color, _Smoothness);
                
                // 环境光
                float3 ambient = SampleSH(normalWS) * 0.1;
                
                // 组合最终颜色（应用阴影衰减）
                float3 finalColor = (diffuse * mainLight.shadowAttenuation + specular) * baseColor.rgb + ambient;
                
                // 应用雾效
                finalColor = MixFog(finalColor, input.fogCoord);
                
                return half4(finalColor, baseColor.a);
            }
            ENDHLSL
        }
        
        // 使用URP内置的ShadowCaster Pass（重要！）
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
    
    // 降级备选
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            
            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            float4 _BaseColor;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.uv) * _BaseColor;
                return col;
            }
            ENDCG
        }
    }
}