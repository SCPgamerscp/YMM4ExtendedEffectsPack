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

static const float BayerMatrix4x4[16] = {
     0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
    12.0/16.0,  4.0/16.0, 14.0/16.0,  6.0/16.0,
     3.0/16.0, 11.0/16.0,  1.0/16.0,  9.0/16.0,
    15.0/16.0,  7.0/16.0, 13.0/16.0,  5.0/16.0
};

float GetDither(float2 screenPos) {
    int x = (int)fmod(abs(screenPos.x), 4.0);
    int y = (int)fmod(abs(screenPos.y), 4.0);
    return BayerMatrix4x4[y * 4 + x] - 0.5;
}

float2 HexCenter(float2 uv) {
    float2 r = float2(1.0, 1.7320508);
    float2 h = r * 0.5;
    float2 a = fmod(abs(uv), r) - h;
    float2 b = fmod(abs(uv - h), r) - h;
    float2 gv = dot(a, a) < dot(b, b) ? a : b;
    return uv - gv;
}

float3 GetGbColor(float l) {
    if (l < 0.25) return float3(0.059, 0.220, 0.059);
    if (l < 0.55) return float3(0.188, 0.384, 0.188);
    if (l < 0.85) return float3(0.545, 0.675, 0.059);
    return float3(0.608, 0.737, 0.059);
}

float4 main(float4 pos:SV_POSITION, float4 posScene:SCENE_POSITION, float4 uv0:TEXCOORD0):SV_Target {
    float2 gUv = GetGlobalUV(posScene, uBounds, uv0.xy);
    float2 res = (uBounds.z > 1.0 && uBounds.w > 1.0) ? uBounds.zw : float2(1920.0, 1080.0);
    
    // ピクセルサイズ (uSize: 2~80px)
    float pSize = max(uSize, 2.0);
    float2 numBlocks = max(res / pSize, float2(1.0, 1.0));
    
    float2 puv_g = gUv;
    float isGrid = 0.0;
    float dotFactor = 1.0;
    
    // 形状判定 (uMode: 0=Square, 1=Circle, 2=Hex)
    if (uMode < 0.5) {
        // 四角形
        float2 cell = floor(gUv * numBlocks);
        puv_g = (cell + 0.5) / numBlocks;
        float2 local = frac(gUv * numBlocks);
        float2 dEdge = min(local, 1.0 - local);
        isGrid = step(min(dEdge.x, dEdge.y), 1.0 / pSize);
    } else if (uMode < 1.5) {
        // 円形ドット
        float2 cell = floor(gUv * numBlocks);
        puv_g = (cell + 0.5) / numBlocks;
        float2 local = frac(gUv * numBlocks) - 0.5;
        float dist = length(local);
        float dotMask = smoothstep(0.48, 0.42, dist);
        dotFactor = lerp(0.15, 1.0, dotMask);
    } else {
        // 六角形
        float2 hexScale = numBlocks * float2(1.0, 1.0 / 1.7320508);
        puv_g = HexCenter(gUv * hexScale) / hexScale;
        float2 localHex = abs(gUv - puv_g) * hexScale;
        float hexDist = max(localHex.x * 0.866025 + localHex.y * 0.5, localHex.y);
        isGrid = step(0.44, hexDist);
    }
    
    float2 delta = puv_g - gUv;
    float4 col = samp(uv0.xy + delta);
    col.rgb *= dotFactor;
    
    // ディザリング (uFlagB > 0.5)
    float2 localPx = (uBounds.z > 1.0 && uBounds.w > 1.0) ? (uv0.xy * uBounds.zw) : (uv0.xy * float2(1920.0, 1080.0));
    float dither = (uFlagB > 0.5) ? (GetDither(localPx) * 0.08) : 0.0;
    
    // レトロ減色 (uCount: 0=Off, 1=16色, 2=64色, 3=GB4階調)
    if (uCount > 2.5) {
        // ゲームボーイ 4階調
        float l = saturate(lum(col.rgb) + dither);
        col.rgb = GetGbColor(l);
    } else if (uCount > 1.5) {
        // 64色 (各チャネル4階調)
        col.rgb = floor(saturate(col.rgb + dither) * 3.0 + 0.5) / 3.0;
    } else if (uCount > 0.5) {
        // 16色 (R:3段階, G:3段階, B:2段階 -> 約18色)
        col.rgb = floor(saturate(col.rgb + dither) * float3(2.0, 2.0, 1.0) + 0.5) / float3(2.0, 2.0, 1.0);
    }
    
    // グリッド線 (uFlagA > 0.5)
    if (uFlagA > 0.5) {
        col.rgb = lerp(col.rgb, float3(0.0, 0.0, 0.0), isGrid * saturate(uMix));
    }
    
    float4 src = samp(uv0.xy);
    return lerp(src, col, saturate(uStrength));
}
