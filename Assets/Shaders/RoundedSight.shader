Shader "Unlit/RoundedSight"
{
    Properties
    {
        _BaseColor("Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "RenderPipeline" = "UniversalRenderPipeline" "Queue" = "Transparent" }

        Pass 
        {
            Tags { "LightMode" = "UniversalForward" }

            ZWrite Off
            Cull Back
            Blend SrcAlpha OneMinusSrcAlpha
            //Blend One One

            HLSLPROGRAM
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            //CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor;
            float _SightAngleCos;
            float _SightAngleTan;
            //CBUFFER_END

            float _RotateRadians;
            float _PixelSize;

            TEXTURE2D_ARRAY(_SightDepthTexture);
            SAMPLER(sampler_SightDepthTexture);
            
            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            float2x2 GetRotateMatrix(float radians)
            {
                float c = cos(radians);
                float s = sin(radians);
                return float2x2(c, s, -s, c);
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                
                half4 col = _BaseColor;

                float2 curPos = input.uv * 2 - 1;
                float2 curPosDir = normalize(curPos.xy);
                float curAngleCos = curPosDir.y;

                if (curAngleCos < _SightAngleCos)
                {
                    return 0;
                }

                int texIndex;
                if(curPos.x > 0)
                {
                    if(curPos.y > 0)
                    {
                        texIndex = 0;
                    }
                    else
                    {
                        texIndex = 1;
                    }
                }
                else
                {
                    if(curPos.y > 0)
                    {
                        texIndex = 3;
                    }
                    else
                    {
                        texIndex = 2;
                    }
                }

                curPosDir = mul(GetRotateMatrix(texIndex * HALF_PI), curPosDir);

                float curAngleTan = curPosDir.x / curPosDir.y;
                float fragmentVal = curAngleTan / 1;
                fragmentVal = fragmentVal * 0.5 + 0.5;
                
                float selfDis = length(curPos);
                float sceneDis = SAMPLE_TEXTURE2D_ARRAY_LOD(_SightDepthTexture, sampler_SightDepthTexture, float2(fragmentVal, 0.5), texIndex, 0).r;

                col.a *= step(selfDis, sceneDis);

                return col;
            }
            ENDHLSL
        }
    }
}