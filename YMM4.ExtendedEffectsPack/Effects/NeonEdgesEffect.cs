using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("サイバーネオン輪郭", new[] { "拡張エフェクト", "描画" }, new[] { "ネオン" }, IsAviUtlSupported = false)]
public sealed class NeonEdgesEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "サイバーネオン輪郭";

    Animation _intensity = new Animation(1.4, 0, 3);
    [Display(Name = "発光強度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 3)]
    public Animation Intensity { get => _intensity; set => Set(ref _intensity, value); }

    Animation _thickness = new Animation(2, 0.5, 6);
    [Display(Name = "輪郭の太さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.5, 6)]
    public Animation Thickness { get => _thickness; set => Set(ref _thickness, value); }

    Animation _flow = new Animation(0.5, 0, 3);
    [Display(Name = "色の流速", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 3)]
    public Animation Flow { get => _flow; set => Set(ref _flow, value); }

    Animation _core = new Animation(0.85, 0, 1);
    [Display(Name = "元画像の残量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Core { get => _core; set => Set(ref _core, value); }

    Color _tint = Color.FromRgb(102, 240, 255);
    [Display(Name = "ベースカラー", GroupName = "外観")]
    [ColorPicker]
    public Color NeonColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<NeonEdgesCustomEffect>(devices, this, d => new NeonEdgesCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Intensity, Thickness, Flow, Core];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Intensity.GetValue(f, len, fps),
            Size = (float)Thickness.GetValue(f, len, fps),
            Speed = (float)Flow.GetValue(f, len, fps),
            Angle = 0f,
            Count = 0f,
            Mix = (float)Core.GetValue(f, len, fps),
            Spread = 0f,
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Core.GetValue(f, len, fps),
            ColorR = NeonColor.R / 255f,
            ColorG = NeonColor.G / 255f,
            ColorB = NeonColor.B / 255f,
        };
    }
}
