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
    return fallbackUv;
}

float2 CenterFromPixels(float px, float py, float4 bounds) {
    float2 c = float2(0.5, 0.5);
    if (bounds.z > 1.0 && bounds.w > 1.0)
        c += float2(px, py) / bounds.zw;
    return c;
}

#define PI 3.14159265

float hash21(float2 p) { return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453); }
float hash11(float n) { return frac(sin(n) * 43758.5453); }
float noise(float2 p) {
    float2 i = floor(p); float2 f = frac(p);
    float a = hash21(i);
    float b = hash21(i + float2(1,0));
    float c = hash21(i + float2(0,1));
    float d = hash21(i + float2(1,1));
    float2 u = f*f*(3-2*f);
    return lerp(lerp(a,b,u.x), lerp(c,d,u.x), u.y);
}
float fbm(float2 p) {
    float v=0, a=0.5;
    [unroll] for (int i=0;i<5;i++) { v+=a*noise(p); p*=2.03; a*=0.5; }
    return v;
}
float4 samp(float2 uv) {
    if (uv.x<0||uv.x>1||uv.y<0||uv.y>1) return 0;
    return InputTexture.Sample(InputSampler, uv);
}
float lum(float3 c) { return dot(c, float3(0.299,0.587,0.114)); }
float3 hsv2rgb(float3 c) {
    float3 p = abs(frac(c.xxx + float3(0, 2.0/3.0, 1.0/3.0))*6-3);
    return c.z * lerp(1, saturate(p-1), c.y);
}

float4 main(float4 pos:SV_POSITION, float4 posScene:SCENE_POSITION, float4 uv0:TEXCOORD0):SV_Target {
    float2 gUv = GetGlobalUV(posScene, uBounds, uv0.xy);
    float aspect = (uBounds.z <= 1.0 || uBounds.w <= 1.0) ? 1.0 : (uBounds.z / uBounds.w);
    float p = saturate(uMix / 100.0);
    float2 c = CenterFromPixels(uAngle, uCount, uBounds);
    float hit = clamp(uStrength, 0.2, 1.0);
    float pulse = 1.0 - pow(abs(p * 2.0 - 1.0), lerp(1.4, 4.5, hit));
    float z = 1.0 + pulse * uSize * 1.6;
    float2 cuv_g = (gUv - c) / z + c;
    float2 delta = cuv_g - gUv;
    float2 cuv = uv0.xy + delta;
    float ch = pulse * uSpread * 0.018;
    float2 dir = normalize((gUv - c) * float2(aspect, 1.0) + 1e-4) / float2(aspect, 1.0);
    float3 col = float3(samp(cuv + dir * ch).r, samp(cuv).g, samp(cuv - dir * ch).b);
    float vig = smoothstep(0.2, 1.1, length((gUv - c) * float2(aspect, 1.0)) * 1.6);
    col *= 1.0 + pulse * 0.18;
    col *= 1.0 - vig * pulse * 0.35;
    return float4(col, 1.0);
}
