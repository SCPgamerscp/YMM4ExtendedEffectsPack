using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("アナモルフィックフレア", new[] { "拡張エフェクト", "描画" }, new[] { "フレア" }, IsAviUtlSupported = false)]
public sealed class AnamorphicFlareEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "アナモルフィックフレア";

    Animation _amount = new Animation(0.8, 0, 2);
    [Display(Name = "フレア量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 2)]
    public Animation Amount { get => _amount; set => Set(ref _amount, value); }

    Animation _streak = new Animation(0.8, 0, 1.5);
    [Display(Name = "水平ストリーク", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1.5)]
    public Animation Streak { get => _streak; set => Set(ref _streak, value); }

    Animation _threshold = new Animation(0.62, 0.2, 0.95);
    [Display(Name = "発光しきい値", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 0.95)]
    public Animation Threshold { get => _threshold; set => Set(ref _threshold, value); }

    Animation _ghosts = new Animation(0.35, 0, 1);
    [Display(Name = "ゴースト", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Ghosts { get => _ghosts; set => Set(ref _ghosts, value); }

    Color _tint = Color.FromRgb(143, 208, 255);
    [Display(Name = "フレア色", GroupName = "外観")]
    [ColorPicker]
    public Color FlareColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<AnamorphicFlareCustomEffect>(devices, this, d => new AnamorphicFlareCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Streak, Threshold, Ghosts];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amount.GetValue(f, len, fps),
            Size = (float)Streak.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = (float)Ghosts.GetValue(f, len, fps),
            Mix = (float)Threshold.GetValue(f, len, fps),
            Spread = 0f,
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Threshold.GetValue(f, len, fps),
            ColorR = FlareColor.R / 255f,
            ColorG = FlareColor.G / 255f,
            ColorB = FlareColor.B / 255f,
        };
    }
}
