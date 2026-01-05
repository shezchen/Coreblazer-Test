Shader "Custom/UI/BackgroundBlurGradient"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        
        // 渐变颜色参数
        _Color1 ("Deep Blue", Color) = (0.1, 0.2, 0.6, 1)
        _Color2 ("Deep Purple", Color) = (0.3, 0.1, 0.5, 1)
        _Color3 ("Dark Blue", Color) = (0.05, 0.15, 0.4, 1)
        
        // 模糊和渐变参数
        _BlurStrength ("Blur Strength", Range(0, 10)) = 2.0
        _GradientScale ("Gradient Scale", Range(0.1, 5)) = 1.5
        _NoiseScale ("Noise Scale", Range(0.1, 10)) = 3.0
        
        // 时间变化参数
        _TimeSpeed ("Time Speed", Range(0, 5)) = 1.0
        _ColorVariation ("Color Variation", Range(0, 1)) = 0.3

        // 噪声控制（新增）
        _NoiseOctaves ("Noise Octaves", Range(1, 6)) = 4
        _NoisePersistence ("Noise Persistence", Range(0, 1)) = 0.5
        _NoiseLacunarity ("Noise Lacunarity", Range(1, 4)) = 2.0
        _WarpStrength ("Warp Strength", Range(0, 2)) = 0.45
        _WarpScale ("Warp Scale", Range(0.1, 10)) = 2.5
        _Seed ("Noise Seed", Float) = 0.0

        // 进阶随机性控制（新增）
        _WarpStrength2 ("Warp Strength 2", Range(0, 2)) = 0.3
        _WarpScale2 ("Warp Scale 2", Range(0.1, 10)) = 3.7
        _RidgedWeight ("Ridged Weight", Range(0, 1)) = 0.35
        _DomainRotation ("Domain Rotation", Range(0, 360)) = 17
        _TimeSeedStrength ("Time Seed Strength", Range(0, 2)) = 0.2
        _Jitter ("Micro Jitter", Range(0, 1)) = 0.05
        
        // UI必需参数
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;
            float4 _MainTex_ST;

            // 自定义参数
            fixed4 _Color1;
            fixed4 _Color2;
            fixed4 _Color3;
            float _BlurStrength;
            float _GradientScale;
            float _NoiseScale;
            float _TimeSpeed;
            float _ColorVariation;

            // 噪声控制参数
            float _NoiseOctaves;
            float _NoisePersistence;
            float _NoiseLacunarity;
            float _WarpStrength;
            float _WarpScale;
            float _Seed;

            // 进阶随机性控制参数
            float _WarpStrength2;
            float _WarpScale2;
            float _RidgedWeight;
            float _DomainRotation; // 度数
            float _TimeSeedStrength;
            float _Jitter;

            v2f vert(appdata_t v)
            {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);
                OUT.worldPosition = v.vertex;
                OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                return OUT;
            }

            // 2D Simplex Noise（Gustavson 版，适配 HLSL）
            float2 mod289(float2 x) { return x - floor(x * (1.0/289.0)) * 289.0; }
            float3 mod289(float3 x) { return x - floor(x * (1.0/289.0)) * 289.0; }
            float3 permute(float3 x) { return mod289((x * 34.0 + 1.0) * x); }

            float snoise(float2 v)
            {
                const float4 C = float4(0.211324865405187, // (3.0-sqrt(3.0))/6.0
                                         0.366025403784439, // 0.5*(sqrt(3.0)-1.0)
                                         -0.577350269189626, // -1.0 + 2.0 * C.x
                                         0.024390243902439); // 1.0 / 41.0

                // 首个角落
                float2 i  = floor(v + dot(v, C.yy));
                float2 x0 = v - i + dot(i, C.xx);

                // 其他角落
                float2 i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
                float2 x1 = x0 - i1 + C.xx;
                float2 x2 = x0 - 1.0 + 2.0 * C.xx;

                // 置乱
                i = mod289(i);
                float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));

                // 梯度与权重
                float3 m = max(0.5 - float3(dot(x0,x0), dot(x1,x1), dot(x2,x2)), 0.0);
                m = m*m; m = m*m;

                float3 x = 2.0 * frac(p * C.www) - 1.0;
                float3 h = abs(x) - 0.5;
                float3 ox = floor(x + 0.5);
                float3 a0 = x - ox;

                // 归一梯度
                m *= 1.79284291400159 - 0.85373472095314 * (a0*a0 + h*h);

                // 点乘
                float3 g;
                g.x = a0.x * x0.x + h.x * x0.y;
                g.y = a0.y * x1.x + h.y * x1.y;
                g.z = a0.z * x2.x + h.z * x2.y;

                return 130.0 * dot(m, g); // 范围约[-1,1]
            }

            // Fractal Brownian Motion（固定最多 6 个 octave，兼容 SM2.0）
            float fbm2D(float2 p, float octaves, float persistence, float lacunarity)
            {
                float value = 0.0;
                float amplitude = 0.5; // 起始幅度
                float frequency = 1.0;
                float totalAmp = 0.0;

                [unroll]
                for (int i = 0; i < 6; i++)
                {
                    float mask = step(i, octaves - 1.0);
                    float n = snoise(p * frequency);
                    value += n * amplitude * mask;
                    totalAmp += amplitude * mask;

                    frequency *= lacunarity;
                    amplitude *= persistence;
                }

                return (totalAmp > 0.0) ? (value / totalAmp) : 0.0; // 仍在[-1,1]
            }

            // Ridged multifractal（更脊化，强调低谷/高峰）
            float ridgedFbm2D(float2 p, float octaves, float persistence, float lacunarity)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;
                float totalAmp = 0.0;

                [unroll]
                for (int i = 0; i < 6; i++)
                {
                    float mask = step(i, octaves - 1.0);
                    float n = snoise(p * frequency);
                    n = 1.0 - abs(n);          // ridged
                    n *= n;                     // 提升对比
                    value += n * amplitude * mask;
                    totalAmp += amplitude * mask;

                    frequency *= lacunarity;
                    amplitude *= persistence;
                }
                return (totalAmp > 0.0) ? (value / totalAmp) : 0.0;
            }

            // 域扭曲（Domain Warp）增强随机性
            float2 domainWarp(float2 p, float t)
            {
                float wx = snoise(p * _WarpScale + t);
                float wy = snoise(p * (_WarpScale * 0.73) - t);
                return p + float2(wx, wy) * _WarpStrength;
            }

            // 二次域扭曲（不同尺度/时间）
            float2 domainWarp2(float2 p, float t)
            {
                float wx = snoise(p * _WarpScale2 - t * 1.1 + 3.1);
                float wy = snoise(p * (_WarpScale2 * 0.61) + t * 0.8 - 1.7);
                return p + float2(wx, wy) * _WarpStrength2;
            }

            // 2D 旋转矩阵
            float2x2 rot2(float a)
            {
                float s = sin(a);
                float c = cos(a);
                return float2x2(c, -s, s, c);
            }

            // 简单 hash 用于微抖动
            float2 hash22(float2 p)
            {
                // 低开销的可重复随机
                float n = sin(dot(p, float2(41.0, 289.0)));
                return frac(float2(262144.0 * n, 32768.0 * n));
            }

            // 种子偏移（保留）
            float2 applySeed(float2 p)
            {
                float s = sin(_Seed * 12.9898);
                float c = cos(_Seed * 78.233);
                return p + float2(s, c);
            }

            fixed4 gaussianBlur(sampler2D tex, float2 uv, float blur)
            {
                fixed4 color = fixed4(0, 0, 0, 0);
                float2 texelSize = 1.0 / _ScreenParams.xy;
                
                float weights[5] = {0.227027, 0.194595, 0.121622, 0.054054, 0.016216};
                
                for(int i = -4; i <= 4; i++)
                {
                    float2 offset = float2(i * texelSize.x * blur, 0);
                    color += tex2D(tex, uv + offset) * weights[abs(i)];
                }
                
                return color;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.texcoord;
                float time = _Time.y * _TimeSpeed;
                
                // 基础噪声域：尺度、旋转、种子、时间微扰
                float2 baseUV = uv * _NoiseScale;
                float angle = radians(_DomainRotation + _Seed * 37.2 + time * _TimeSeedStrength * 10.0);
                baseUV = mul(rot2(angle), baseUV);
                baseUV = applySeed(baseUV);
                baseUV += (hash22(baseUV + time * 0.23) - 0.5) * (_Jitter * 0.5);

                // 双重域扭曲并混合
                float2 warpedA = domainWarp(baseUV, time * 0.10);
                float2 warpedB = domainWarp2(baseUV, -time * 0.11);
                float2 warpedUV = lerp(warpedA, warpedB, 0.5);

                // 双噪声：标准 FBM 与 Ridged FBM，以提升随机结构
                float nRegular = fbm2D(warpedUV, _NoiseOctaves, _NoisePersistence, _NoiseLacunarity);
                float nRidged  = ridgedFbm2D(warpedUV * 1.37 + 2.1, _NoiseOctaves, _NoisePersistence, _NoiseLacunarity);
                float nMix = lerp(nRegular, nRidged, _RidgedWeight);

                // 第二路噪声用于正交方向的变化
                float nAux = fbm2D(warpedUV * 1.7 + 3.1, _NoiseOctaves, _NoisePersistence, _NoiseLacunarity);

                // 映射到[0,1]
                nMix = nMix * 0.5 + 0.5;
                nAux = nAux * 0.5 + 0.5;
                
                // 创建渐变坐标
                float2 gradientUV = (uv - 0.5) * _GradientScale;
                float gradient1 = length(gradientUV) + nMix * 0.5;
                float gradient2 = dot(mul(rot2(0.785398), gradientUV), float2(1, 0)) + nAux * 0.3; // 约 45° 方向
                
                // 时间变化的颜色混合权重
                float timeOffset1 = sin(time * 0.7) * 0.5 + 0.5;
                float timeOffset2 = cos(time * 0.5 + 2.0) * 0.5 + 0.5;
                float timeOffset3 = sin(time * 0.3 + 4.0) * 0.5 + 0.5;
                
                // 计算颜色混合权重
                float weight1 = saturate(1.0 - gradient1 * 0.8 + timeOffset1 * _ColorVariation);
                float weight2 = saturate(gradient2 * 0.6 + timeOffset2 * _ColorVariation);
                float weight3 = saturate((1.0 - weight1 - weight2) + timeOffset3 * _ColorVariation * 0.5);
                
                // 归一化权重
                float totalWeight = weight1 + weight2 + weight3 + 1e-5;
                weight1 /= totalWeight;
                weight2 /= totalWeight;
                weight3 /= totalWeight;
                
                // 混合颜色
                fixed4 finalColor = _Color1 * weight1 + _Color2 * weight2 + _Color3 * weight3;
                
                // 添加柔和的边缘淡化
                float2 edgeFade = abs(uv - 0.5) * 2.0;
                float fade = 1.0 - saturate(pow(max(edgeFade.x, edgeFade.y), 1.5));
                
                // 应用模糊效果（通过降低对比度模拟）
                finalColor.rgb = lerp(finalColor.rgb, finalColor.rgb * 0.7 + 0.3 * (finalColor.r + finalColor.g + finalColor.b) / 3.0, _BlurStrength * 0.1);
                
                // 应用UI颜色和透明度
                finalColor *= IN.color;
                finalColor.a *= fade;
                
                // UI裁剪
                finalColor.a *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
                
                #ifdef UNITY_UI_ALPHACLIP
                clip(finalColor.a - 0.001);
                #endif

                return finalColor;
            }
        ENDCG
        }
    }
}