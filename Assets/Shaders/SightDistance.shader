Shader "Hidden/SightDistance"
{
    Properties
    {
        
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalRenderPipeline" }

        Pass 
        {
            Name "Sight Distance"
            Tags { "LightMode" = "UniversalForward" }

            ZWrite On
            Cull Back
            //ColorMask 0

            HLSLPROGRAM
            #pragma multi_compile_instancing
            
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float4 positionVS : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                output.positionVS = mul(UNITY_MATRIX_MV, float4(input.positionOS.xyz, 1));
                output.positionCS = mul(UNITY_MATRIX_P, output.positionVS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                float distance = length(input.positionVS.xyz);

                distance = saturate(distance / _ProjectionParams.z);

                return half4(distance, 0, 0, 0);
            }
            ENDHLSL
        }
    }
}
