Shader "Custom/PortalGameEffectFittedURP"
{
    Properties
    {
        [HDR] _GlowColor ("Aura Color", Color) = (0, 0.5, 1, 1)
        [HDR] _CoreColor ("Core Color", Color) = (1, 1, 1, 1)
        
        [Space(10)]
        _MainTex ("Internal Texture", 2D) = "black" {}
        [Toggle] _FlipY ("Flip Image", Float) = 0
        
        [Space(10)]
        _Radius ("Maximum Radius", Range(0.1, 0.5)) = 0.345
        _Width ("Portal Width (Oval)", Range(0.2, 1.0)) = 0.915
        
        _Thickness ("Aura Thickness", Range(0.01, 0.3)) = 0.189
        _EdgeSoftness ("Internal Texture Softness", Range(0.001, 0.1)) = 0.02
        
        [Space(10)]
        _Speed ("Energy Speed", Range(0, 5)) = 2.0
        _Distortion ("Edge Agitation", Range(0, 0.2)) = 0.1
        _Turbulence ("Plasma Detail", Range(1, 10)) = 1.0
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
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
                float4 screenPos : TEXCOORD1;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                half4 _GlowColor;
                half4 _CoreColor;
                float _FlipY;
                float _Radius;
                float _Width;
                float _Thickness;
                float _EdgeSoftness;
                float _Speed;
                float _Distortion;
                float _Turbulence;
            CBUFFER_END

            float _IsPortalCamera;

            float hash(float2 p)
            {
                p = frac(p * float2(127.1, 311.7));
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0.0,0.0)), hash(i + float2(1.0,0.0)), u.x),
                            lerp(hash(i + float2(0.0,1.0)), hash(i + float2(1.0,1.0)), u.x), u.y);
            }

            float fbm(float2 p)
            {
                float v = 0.0;
                float amplitude = 0.5;
                float2x2 rot = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
                for (int i = 0; i < 3; ++i) {
                    v += amplitude * noise(p);
                    p = mul(rot, p) * 2.0 + float2(100.0, 100.0);
                    amplitude *= 0.5;
                }
                return v;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;

                OUT.screenPos = ComputeScreenPos(OUT.positionCS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 centerUV = IN.uv - 0.5;
                
                float2 shapeUV = centerUV;
                shapeUV.x /= _Width; 
                float dist = length(shapeUV);
                
                float2 dir = normalize(shapeUV);
                float2 noiseUV = dir * _Turbulence + float2(dist * 3.0, -_Time.y * _Speed);
                float noiseVal = fbm(noiseUV);

                float distortedDist = dist + (noiseVal - 0.5) * _Distortion;

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                if (_FlipY == 1.0) 
                {
                    screenUV.y = 1.0 - screenUV.y;
                }

                half4 texColor = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, screenUV);
                
                float tunnelDepth = 0.5 / max(dist, 0.05); 
                float tunnelAngle = atan2(shapeUV.y, shapeUV.x);
                float2 tunnelUVs = float2(tunnelAngle * 2.0, tunnelDepth - _Time.y * _Speed * 1.5);
                float tunnelPlasma = fbm(tunnelUVs);
                half3 vortexColor = _GlowColor.rgb * tunnelPlasma * smoothstep(0.0, 0.4, dist) * 2.5;

                texColor.rgb = lerp(texColor.rgb, vortexColor, _IsPortalCamera);
                
                float insideMask = 1.0 - smoothstep(_Radius - _EdgeSoftness, _Radius, distortedDist);
                float distanceFromEdge = abs(distortedDist - _Radius);
                float aura = smoothstep(_Thickness, 0.0, distanceFromEdge);
                float core = smoothstep(_Thickness * 0.2, 0.0, distanceFromEdge);
                
                half3 ringGlow = (_GlowColor.rgb * aura) + (_CoreColor.rgb * core * 1.5);
                half3 finalRGB = (texColor.rgb * insideMask) + ringGlow;
                half finalAlpha = max(texColor.a * insideMask, aura);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
}