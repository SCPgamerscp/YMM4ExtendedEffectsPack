Texture2D InputTexture : register(t0);
Texture2D InputTexture1 : register(t1);
SamplerState InputSampler : register(s0);

cbuffer Constants : register(b0)
{
    float uStrength : packoffset(c0.x);
    float uSize     : packoffset(c0.y);
    float uSpeed    : packoffset(c0.z);
    float uAngle    : packoffset(c0.w);
    float uCount    : packoffset(c1.x);
    float uMix      : packoffset(c1.y);
    float uSpread   : packoffset(c1.z);
    float uMode     : packoffset(c1.w);
    float uFlagA    : packoffset(c2.x);
    float uFlagB    : packoffset(c2.y);
    float uTime     : packoffset(c2.z);
    float uProgress : packoffset(c2.w);
    float uColorR   : packoffset(c3.x);
    float uColorG   : packoffset(c3.y);
    float uColorB   : packoffset(c3.z);
    float uPad      : packoffset(c3.w);
};

float4 samp0(float2 uv) {
    if (uv.x<0||uv.x>1||uv.y<0||uv.y>1) return 0;
    return InputTexture.Sample(InputSampler, uv);
}
float4 samp1(float2 uv) {
    if (uv.x<0||uv.x>1||uv.y<0||uv.y>1) return 0;
    return InputTexture1.Sample(InputSampler, uv);
}

float4 main(float4 pos:SV_POSITION, float4 posScene:SCENE_POSITION, float4 uv0:TEXCOORD0):SV_Target {
    float2 uv = uv0.xy;
    float4 src = samp0(uv);
    if (uFlagB < 0.5) return src;
    float2 dir = float2(1, 0.15) * uSpread * 0.001;
    float sc = 1 + (uSize / 100);
    float2 ue = (uv - 0.5) / sc + 0.5;
    float3 ghost = float3(samp1(uv + dir).r, samp1(uv).g, samp1(uv - dir).b);
    float3 echo  = float3(samp1(ue + dir).r, samp1(ue).g, samp1(ue - dir).b);
    ghost = lerp(ghost, echo, 0.45);
    float fade = saturate(uMix);
    return float4(lerp(src.rgb, ghost, fade), src.a);
}
