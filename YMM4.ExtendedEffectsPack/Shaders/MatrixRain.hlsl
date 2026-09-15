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

float DrawGlyph(float2 cellUv, float charSeed, float isBinary) {
    if (cellUv.x < 0.15 || cellUv.x > 0.85 || cellUv.y < 0.12 || cellUv.y > 0.88) {
        return 0.0;
    }
    if (isBinary > 0.5) {
        // 0 または 1
        if (charSeed > 0.5) {
            // "1": 中央の縦線
            return step(abs(cellUv.x - 0.5), 0.12);
        } else {
            // "0": 外枠
            float2 d = abs(cellUv - 0.5);
            return step(max(d.x * 1.3, d.y), 0.35) * (1.0 - step(max(d.x * 1.3, d.y), 0.18));
        }
    } else {
        // マトリックス風デジタル文字パターン (3x4 ドットマトリクス)
        float2 dotPos = floor(cellUv * float2(3.0, 4.0));
        float p = hash21(dotPos + charSeed * 17.0);
        return step(0.42, p);
    }
}

float4 main(float4 pos:SV_POSITION, float4 posScene:SCENE_POSITION, float4 uv0:TEXCOORD0):SV_Target {
    float2 gUv = GetGlobalUV(posScene, uBounds, uv0.xy);
    float2 res = (uBounds.z > 1.0 && uBounds.w > 1.0) ? uBounds.zw : float2(1920.0, 1080.0);
    
    // グリフサイズ (uSize: 8~48px)
    float glyphW = max(uSize, 8.0);
    float glyphH = glyphW * 1.4;
    float2 numGrid = max(res / float2(glyphW, glyphH), float2(1.0, 1.0));
    
    float2 cell = floor(gUv * numGrid);
    float2 cellUv = frac(gUv * numGrid);
    
    // 列ごとのランダム速度
    float colSeed = hash11(cell.x * 0.137 + 0.5);
    float fallSpd = (0.6 + colSeed * 0.8) * max(uSpeed, 0.1) * 2.5;
    
    // 落下先頭位置
    float trailLen = max(uCount, 5.0);
    float totalH = numGrid.y + trailLen;
    float headY = fmod(uTime * fallSpd * 5.0 + colSeed * 100.0, totalH);
    
    // 先頭からの距離
    float distFromHead = headY - cell.y;
    float inTrail = step(0.0, distFromHead) * step(distFromHead, trailLen);
    float intensity = inTrail * pow(saturate(1.0 - (distFromHead / trailLen)), 1.3);
    float isHead = step(distFromHead, 1.1) * step(0.0, distFromHead);
    
    // 文字のチラつき変化
    float charTick = floor(uTime * 6.0 + colSeed * 13.0);
    float charSeed = hash21(cell + charTick);
    
    float isBinary = (uMode > 1.5) ? 1.0 : 0.0;
    float glyph = DrawGlyph(cellUv, charSeed, isBinary);
    
    float3 tint = float3(uColorR, uColorG, uColorB);
    float3 rainRgb = lerp(tint * intensity, float3(1.0, 1.0, 1.0), isHead * intensity);
    
    float4 src = samp(uv0.xy);
    float3 finalRgb = src.rgb;
    float finalA = src.a;
    
    if (uMode < 0.5 || uMode > 1.5) {
        // Mode 0: 背景オーバーレイ / Mode 2: 01バイナリ
        float glyphLight = glyph * intensity;
        finalRgb = src.rgb + rainRgb * glyphLight;
        finalA = max(src.a, glyphLight * 0.9);
    } else {
        // Mode 1: 元画像コード分解 (Ascii)
        float2 puv_g = (cell + 0.5) / numGrid;
        float4 cellSrc = samp(uv0.xy + (puv_g - gUv));
        float l = lum(cellSrc.rgb);
        float codeGlow = glyph * l * (0.35 + 0.65 * intensity);
        finalRgb = lerp(tint * codeGlow, float3(1.0, 1.0, 1.0), isHead * codeGlow);
        finalA = cellSrc.a;
    }
    
    return float4(lerp(src.rgb, finalRgb, saturate(uStrength)), finalA);
}
