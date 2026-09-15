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
    float2 texelSize = uv0.zw;
    if (texelSize.x <= 0.0) {
        texelSize.x = 1.0 / max(uBounds.z, 1080.0);
    }
    
    // 水平ストリークの広がり (uSize: デフォルト0.8, スライダー0〜1.5)
    // シャープで細く長いシネマティックストリークを形成
    float stepPx = max(uSize * 6.0, 1.0);
    float2 stepUv = float2(stepPx * texelSize.x, 0.0);
    
    // 発光しきい値 (uMix: デフォルト0.62, スライダー0.2〜0.95)
    // 肌色や中間調（顔の目・鼻・口）を発光させず完全に保護するため、
    // threshold以上の高輝度超過分のみを抽出（顔の白飛びを完全防止）
    float threshold = clamp(uMix, 0.45, 0.98);
    
    float3 streak = 0;
    float wsum = 0;
    
    // 1. コアストリーク (35タップ: -17 .. +17)
    [unroll] for (int i = -17; i <= 17; i++) {
        float norm = (float)i / 17.0;
        float w = exp(-norm * norm * 3.5);
        
        float2 sampleUv = uv + (float)i * stepUv;
        float4 s = samp(sampleUv);
        
        float lumVal = lum(s.rgb);
        float excess = max(lumVal - threshold, 0.0);
        float lm = pow(excess / max(1.0 - threshold, 0.01), 1.8);
        
        streak += s.rgb * lm * w;
        wsum += w;
    }
    streak /= max(wsum, 1e-4);
    
    // 2. 広域ストリーク (25タップ: -12 .. +12)
    // 左右遠くまでスーッと美しく尾を引くシネマティックロングテール
    float3 wideStreak = 0;
    float wideWsum = 0;
    float2 wideStepUv = float2(stepPx * 4.0 * texelSize.x, 0.0);
    [unroll] for (int j = -12; j <= 12; j++) {
        float wNorm = (float)j / 12.0;
        float w = exp(-wNorm * wNorm * 3.5);
        
        float2 sampleUv = uv + (float)j * wideStepUv;
        float4 s = samp(sampleUv);
        
        float lumVal = lum(s.rgb);
        float excess = max(lumVal - threshold, 0.0);
        float lm = pow(excess / max(1.0 - threshold, 0.01), 1.8);
        
        wideStreak += s.rgb * lm * w;
        wideWsum += w;
    }
    wideStreak /= max(wideWsum, 1e-4);
    
    float3 totalStreak = streak * 0.65 + wideStreak * 0.35;
    
    // 3. アナモルフィック色収差サテライト (uCount: Ghosts, デフォルト0.35)
    // 反転ゴーストは完全撤去！実レンズ特有の、赤と青がわずかに分光する光学色収差を付加
    float3 chromaStreak = totalStreak;
    if (uCount > 0.02) {
        float chromaOffset = stepPx * 2.0 * texelSize.x * uCount;
        float redSample = samp(uv + float2(chromaOffset, 0.0)).r;
        float blueSample = samp(uv - float2(chromaOffset, 0.0)).b;
        chromaStreak.r = lerp(chromaStreak.r, redSample, uCount * 0.35);
        chromaStreak.b = lerp(chromaStreak.b, blueSample, uCount * 0.35);
    }
    
    float3 flareColor = float3(uColorR, uColorG, uColorB);
    float3 flareTotal = chromaStreak * flareColor * (uStrength * 2.2);
    
    // 加算合成
    float3 acc = src.rgb + flareTotal;
    
    // 画像枠外でも光が見えるようにアルファを計算
    float flareAlpha = saturate(lum(flareTotal) * 1.5);
    float outAlpha = max(src.a, flareAlpha);
    
    return float4(acc, outAlpha);
}