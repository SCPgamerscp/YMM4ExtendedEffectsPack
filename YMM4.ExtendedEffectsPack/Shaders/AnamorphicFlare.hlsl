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
    float2 uv = uv0.xy;
    float4 src = samp(uv);
    
    // Direct2D が提供する正確なテクセルサイズ (uv0.zw)
    // プレビューの縮小表示でも出力時でも、常にその描画解像度の1テクセルを正確に指す
    float2 texelSize = uv0.zw;
    if (texelSize.x <= 0.0) {
        texelSize.x = 1.0 / max(uBounds.z, 1080.0);
    }
    
    // 水平ストリークの広がり
    // uSize: デフォルト0.8（スライダー0〜1.5）
    // 等間隔ガウスサンプリングにより、縦線の縞模様（バンディング）を完全防止
    float stepPx = max(uSize * 5.0, 1.0);
    float2 stepUv = float2(stepPx * texelSize.x, 0.0);
    
    float3 streak = 0;
    float wsum = 0;
    
    // 1. コアストリーク (35タップ: -17 .. +17)
    [unroll] for (int i = -17; i <= 17; i++) {
        float norm = (float)i / 17.0;
        float w = exp(-norm * norm * 3.0);
        
        float2 sampleUv = uv + (float)i * stepUv;
        float4 s = samp(sampleUv);
        
        float lumVal = lum(s.rgb);
        float lm = smoothstep(uMix - 0.08, uMix + 0.12, lumVal);
        
        streak += s.rgb * lm * w;
        wsum += w;
    }
    streak /= max(wsum, 1e-4);
    
    // 2. 広域ストリーク (25タップ: -12 .. +12)
    // 中心から遠くまで滑らかに尾を引くシネマティックロングテール
    float3 wideStreak = 0;
    float wideWsum = 0;
    float2 wideStepUv = float2(stepPx * 3.5 * texelSize.x, 0.0);
    [unroll] for (int j = -12; j <= 12; j++) {
        float wNorm = (float)j / 12.0;
        float w = exp(-wNorm * wNorm * 3.2);
        
        float2 sampleUv = uv + (float)j * wideStepUv;
        float4 s = samp(sampleUv);
        
        float lumVal = lum(s.rgb);
        float lm = smoothstep(uMix - 0.05, uMix + 0.15, lumVal);
        
        wideStreak += s.rgb * lm * w;
        wideWsum += w;
    }
    wideStreak /= max(wideWsum, 1e-4);
    
    float3 totalStreak = streak * 0.65 + wideStreak * 0.35;
    
    // レンズゴースト (uCount: Ghosts, デフォルト0.35)
    float3 ghostCol = 0;
    if (uCount > 0.02) {
        float2 ghostUv = float2(1.0 - uv.x, uv.y);
        float4 gs = samp(ghostUv);
        float glm = smoothstep(uMix * 1.05, 1.0, lum(gs.rgb));
        ghostCol = gs.rgb * glm * uCount * 0.35;
    }
    
    float3 flareColor = float3(uColorR, uColorG, uColorB);
    float3 flareTotal = (totalStreak + ghostCol) * flareColor * (uStrength * 1.8);
    
    float3 acc = src.rgb + flareTotal;
    return float4(acc, src.a);
}