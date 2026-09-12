using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

internal sealed class BlinkCustomEffect : D2D1CustomShaderEffectBase, IPackShader
{
    public BlinkCustomEffect(IGraphicsDevicesAndContext devices) : base(Create<Impl>(devices)) {}

    public void SetUniforms(PackUniforms u)
    {
        SetValue(0, u.Strength); SetValue(1, u.Size); SetValue(2, u.Speed); SetValue(3, u.Angle);
        SetValue(4, u.Count); SetValue(5, u.Mix); SetValue(6, u.Spread); SetValue(7, u.Mode);
        SetValue(8, u.FlagA); SetValue(9, u.FlagB); SetValue(10, u.Time); SetValue(11, u.Progress);
        SetValue(12, u.ColorR); SetValue(13, u.ColorG); SetValue(14, u.ColorB);
    }

    [CustomEffect(1)]
    sealed class Impl : PackEffectImplBase<Impl>
    {
        public Impl() : base("Blink") {}
    }
}
