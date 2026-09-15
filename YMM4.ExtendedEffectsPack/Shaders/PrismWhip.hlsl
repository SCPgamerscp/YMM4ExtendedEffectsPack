Texture2D InputTexture : register(t0);
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
    float4 uBounds  : packoffset(c4);
};

float2 GetGlobalUV(float4 posScene, float4 bounds, float2 fallbackUv) {
    if (bounds.z > 1.0 && bounds.w > 1.0)
        return (posScene.xy - bounds.xy) / bounds.zw;
    return fallbackUv;
}

#define PI 3.14159265

float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
float noise(float2 p) {
    float2 i = floor(p); float2 f = frac(p);
    float a = hash21(i);
    float b = hash21(i + float2(1,0));
    float c = hash21(i + float2(0,1));
    float d = hash21(i + float2(1,1));
    float2 u = f*f*(3-2*f);
    return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
}
float4 samp(float2 uv) {
    if (uv.x<0||uv.x>1||uv.y<0||uv.y>1) return 0;
    return InputTexture.Sample(InputSampler, uv);
}
float2 mirrored(float2 uv) { return 1-abs(frac(uv*.5)*2-1); }

float4 main(float4 pos:SV_POSITION, float4 posScene:SCENE_POSITION, float4 uv0:TEXCOORD0):SV_Target {
    float2 gUv = GetGlobalUV(posScene, uBounds, uv0.xy);
    float aspect = (uBounds.z <= 1.0 || uBounds.w <= 1.0) ? 1.0 : (uBounds.z / uBounds.w);
    float p = saturate(uMix / 100.0);
    float env = sin(p * 3.14159265);
    float ang = uAngle * 0.01745329251;
    float2 dir = float2(cos(ang), sin(ang));
    float2 q_g = (gUv - 0.5) / (1.0 + uSize / 100.0 * env) + 0.5;
    float2 split = dir * uSpread * 0.0015 * env;
    float3 acc = 0.0;
    [unroll] for (int i = 0; i < 12; i++) {
        float2 offset = dir * ((i / 11.0) - 0.5) * uStrength * env * 0.35;
        float2 u_g = q_g + dir * env * 0.12 + offset;
        float2 delta = u_g - gUv;
        float2 u = uv0.xy + delta;
        acc += float3(samp(u + split).r, samp(u).g, samp(u - split).b);
    }
    return float4(acc / 12.0, 1.0);
}
