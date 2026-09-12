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
    float2 uv=uv0.xy; float h=1-uv.y;
    float2 nUV=uv*float2(3,5.5)-float2(0,uTime*uSpeed);
    float n1=fbm(nUV); float n2=fbm(nUV*2.1+1.7);
    float2 distort=float2(n1-0.5, -abs(n2))*(uStrength*0.0045*(0.25+h*1.4));
    float4 src=samp(uv+distort);
    if (uFlagA>0.5) {
        float t=saturate(lum(src.rgb)*1.1+h*0.35);
        float3 pal=lerp(float3(0.12,0.02,0), lerp(float3(0.9,0.18,0.02), float3(1,0.92,0.35), t), t);
        if (uMode>0.5 && uMode<1.5) pal=lerp(float3(0,0.02,0.12), lerp(float3(0.05,0.35,0.95), float3(0.75,0.95,1), t), t);
        if (uMode>1.5) pal=lerp(float3(0.08,0,0.12), lerp(float3(0.55,0.05,0.85), float3(0.95,0.55,1), t), t);
        src.rgb=lerp(src.rgb, pal, 0.82);
    }
    float burn=uMix/100;
    float dissolve=smoothstep(burn-0.12, burn+0.18, h+n2*0.25);
    src.rgb*=1-dissolve*burn;
    return src;
}