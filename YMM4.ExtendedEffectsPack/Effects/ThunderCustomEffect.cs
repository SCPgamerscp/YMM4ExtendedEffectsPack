using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

internal sealed class ThunderCustomEffect : D2D1CustomShaderEffectBase, IPackShader
{
    public ThunderCustomEffect(IGraphicsDevicesAndContext devices) : base(Create<Impl>(devices)) {}

    public void SetUniforms(PackUniforms u)
    {
        SetValue(0, u.Strength); SetValue(1, u.Size); SetValue(2, u.Speed); SetValue(3, u.Angle);
        SetValue(4, u.Count); SetValue(5, u.Mix); SetValue(6, u.Spread); SetValue(7, u.Mode);
        SetValue(8, u.FlagA); SetValue(9, u.FlagB); SetValue(10, u.Time); SetValue(11, u.Progress);
        SetValue(12, u.ColorR); SetValue(13, u.ColorG); SetValue(14, u.ColorB);
    }

    [CustomEffect(1)]
    sealed class Impl : D2D1CustomShaderEffectImplBase<Impl>
    {
        PackUniforms _cb;
        public Impl() : base(ShaderResourceUri.Get("Thunder")) {}
        [CustomEffectProperty(PropertyType.Float, 0)] public float Strength { get => _cb.Strength; set { _cb.Strength = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 1)] public float Size { get => _cb.Size; set { _cb.Size = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 2)] public float Speed { get => _cb.Speed; set { _cb.Speed = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 3)] public float Angle { get => _cb.Angle; set { _cb.Angle = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 4)] public float Count { get => _cb.Count; set { _cb.Count = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 5)] public float Mix { get => _cb.Mix; set { _cb.Mix = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 6)] public float Spread { get => _cb.Spread; set { _cb.Spread = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 7)] public float Mode { get => _cb.Mode; set { _cb.Mode = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 8)] public float FlagA { get => _cb.FlagA; set { _cb.FlagA = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 9)] public float FlagB { get => _cb.FlagB; set { _cb.FlagB = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 10)] public float Time { get => _cb.Time; set { _cb.Time = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 11)] public float Progress { get => _cb.Progress; set { _cb.Progress = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 12)] public float ColorR { get => _cb.ColorR; set { _cb.ColorR = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 13)] public float ColorG { get => _cb.ColorG; set { _cb.ColorG = value; UpdateConstants(); } }
        [CustomEffectProperty(PropertyType.Float, 14)] public float ColorB { get => _cb.ColorB; set { _cb.ColorB = value; UpdateConstants(); } }
        protected override void UpdateConstants() => drawInformation?.SetPixelShaderConstantBuffer(_cb);
    }
}
