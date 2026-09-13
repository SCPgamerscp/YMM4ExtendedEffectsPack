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
    if (bounds.z <= 1.0 || bounds.w <= 1.0) {
        return fallbackUv;
    }
    return (posScene.xy - bounds.xy) / bounds.zw;
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
    
    float cycle = frac(uTime * max(uSpeed, 0.01));
    float pulse = 0.0;
    
    // 鼓動モード (uMode: 0=Ecg, 1=Sine)
    if (uMode < 0.5) {
        // ドッ・クン (2段拍動)
        float p1 = saturate(cycle / 0.16);
        float b1 = sin(p1 * 3.14159265) * exp(-cycle * 12.0);
        float c2 = max(0.0, cycle - 0.18);
        float p2 = saturate(c2 / 0.16);
        float b2 = sin(p2 * 3.14159265) * exp(-c2 * 12.0) * 0.65;
        pulse = max(b1, b2);
    } else {
        // シンプル弾み
        float p = saturate(cycle / 0.35);
        pulse = sin(p * 3.14159265) * exp(-cycle * 5.0);
    }
    
    // 拡縮強度
    float scale = max(uStrength, uSize);
    float z = 1.0 + pulse * scale;
    float2 zuv_g = (gUv - 0.5) / z + 0.5;
    float2 delta = zuv_g - gUv;
    float2 zuv = uv0.xy + delta;
    
    // 色収差ブレ (uSpread: 0~15)
    float2 chromaDir = normalize(gUv - 0.5 + 1e-5);
    float2 cShift = chromaDir * (pulse * uSpread * 0.0015);
    float rCol = samp(zuv + cShift).r;
    float gCol = samp(zuv).g;
    float bCol = samp(zuv - cShift).b;
    float aCol = samp(zuv).a;
    float4 col = float4(rCol, gCol, bCol, aCol);
    
    // 周辺フラッシュ (uMix, FlashColor)
    float dist = length((gUv - 0.5) * float2(aspect, 1.0));
    float flashMask = smoothstep(0.25, 0.9, dist) * pulse * saturate(uMix);
    float3 flashCol = float3(uColorR, uColorG, uColorB);
    col.rgb = col.rgb + flashCol * flashMask * 1.5;
    
    return col;
}
