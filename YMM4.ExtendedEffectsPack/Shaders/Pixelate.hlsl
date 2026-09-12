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
};

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
    float2 uv=uv0.xy;
    float2 block=max(uSize,2)/float2(1920,1080);
    float2 idx=floor(uv/block);
    float2 center=(idx+0.5)*block;
    float4 col=samp(center);
    if (uCount>0.5 && uCount<1.5) col.rgb=floor(col.rgb*4+0.5)/4;
    else if (uCount>1.5 && uCount<2.5) col.rgb=floor(col.rgb*8+0.5)/8;
    else if (uCount>2.5) {
        float g=floor(lum(col.rgb)*3+0.5)/3;
        col.rgb=lerp(float3(0.06,0.15,0.08), float3(0.61,0.73,0.16), g);
    }
    float2 local=(uv-center)/block;
    if (uMode>0.5 && uMode<1.5 && length(local)>0.46) col.rgb=0.05;
    if (uFlagA>0.5) {
        float2 bd=abs(frac(uv/block)-0.5);
        if (max(bd.x,bd.y)>0.46) col.rgb=lerp(col.rgb,0.02,uMix);
    }
    return float4(col.rgb,1);
}