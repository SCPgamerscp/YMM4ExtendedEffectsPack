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
    float2 uv=uv0.xy; float3 src=samp(uv).rgb;
    float ang=uAngle*0.01745329251;
    float2x2 rot=float2x2(cos(ang),-sin(ang),sin(ang),cos(ang));
    float2x2 inv=float2x2(cos(ang),sin(ang),-sin(ang),cos(ang));
    float cell=max(uSize,2);
    float2 px=uv*float2(1920,1080);
    float3 col;
    if (uMode<0.5) {
        float2 p=mul(rot,px);
        float2 g=floor(p/cell);
        float2 gc=(g+0.5)*cell;
        float2 suv=mul(inv,gc)/float2(1920,1080);
        float L=lum(samp(suv).rgb);
        L=saturate((L-0.5)*(1+uStrength*2)+0.5);
        float dist=length(p-gc)/(cell*0.5);
        float dotv=1-smoothstep(L*0.92, L*0.92+0.08, dist);
        float3 ink=float3(uColorR,uColorG,uColorB);
        float3 paper=uFlagA>0.5 ? float3(0.93,0.90,0.84) : src;
        col=lerp(paper, ink, dotv);
    } else {
        float3 acc=0;
        float angs[4]={0.2618,1.309,0.7854,0};
        [unroll] for (int i=0;i<4;i++) {
            float a=angs[i];
            float2x2 r=float2x2(cos(a),-sin(a),sin(a),cos(a));
            float2x2 ri=float2x2(cos(a),sin(a),-sin(a),cos(a));
            float2 p=mul(r,px); float2 g=floor(p/cell); float2 gc=(g+0.5)*cell;
            float2 suv=mul(ri,gc)/float2(1920,1080);
            float3 s=samp(suv).rgb;
            float ch=i==0?s.g:i==1?s.r:i==2?s.b:lum(s);
            ch=saturate((ch-0.5)*(1+uStrength)+0.5);
            float dist=length(p-gc)/(cell*0.5);
            float dotv=1-smoothstep(ch*0.9, ch*0.9+0.1, dist);
            float3 ink=i==0?float3(0,0.85,0.85):i==1?float3(0.95,0,0.7):i==2?float3(1,0.92,0):float3(0.08,0.08,0.08);
            acc+=ink*dotv;
        }
        col=1-acc*0.7;
    }
    return float4(col,1);
}
