Shader "Scooper/Dirt"
{
    Properties
    {
        _QuadSize ("Quad Size", Float) = 1
        _ColorA ("Color A", Color) = (0.5, 0.5, 0.5, 1)
        _ColorB ("Color B", Color) = (1, 1, 0, 1)
    }

    HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Random.hlsl"

struct Attributes
{
    float4 positionOS : POSITION;
    uint vertexID : SV_VertexID;
};

struct Varyings
{
    float4 positionCS : SV_POSITION;
    float4 color : COLOR;
};

TEXTURE2D(_BodyTex);
SAMPLER(sampler_BodyTex);
float _QuadSize;
int _BodyCount;
float4 _ColorA;
float4 _ColorB;

void Vertex(float4 positionOS : POSITION,
            uint vertexID : SV_VertexID,
            out float4 outPositionCS : SV_POSITION,
            out float4 outColor : COLOR)
{
    uint quadId = vertexID / 4;
    float2 uv = float2((quadId + 0.5) / _BodyCount, 0.5);

    float4 data = SAMPLE_TEXTURE2D_LOD(_BodyTex, sampler_BodyTex, uv, 0);
    float2 pos = data.xy;
    float angle = data.z;
    float enabled = data.w;

    float size = _QuadSize * enabled;
    float2 local = positionOS.xy * size;

    float s = sin(angle);
    float c = cos(angle);
    float2 rotated = float2(c * local.x - s * local.y,
                            s * local.x + c * local.y);

    float3 world = TransformObjectToWorld(float3(pos + rotated, 0));
    outPositionCS = TransformWorldToHClip(world);

    float rnd = GenerateHashedRandomFloat(quadId);
    outColor = lerp(_ColorA, _ColorB, rnd);
}

half4 Fragment(float4 positionCS : SV_POSITION,
               float4 color : COLOR) : SV_Target
{
    return color;
}

    ENDHLSL

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}
