using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("集中線", new[] { "拡張エフェクト", "演出" }, new[] { "漫画" }, IsAviUtlSupported = false)]
public sealed class SpeedLinesEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "集中線";

    Animation _density = new Animation(56, 8, 120);
    [Display(Name = "密度", GroupName = "基本")]
    [AnimationSlider("F2", "", 8, 120)]
    public Animation Density { get => _density; set => Set(ref _density, value); }

    Animation _length = new Animation(0.52, 0.15, 0.9);
    [Display(Name = "線の長さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.15, 0.9)]
    public Animation Length { get => _length; set => Set(ref _length, value); }

    Animation _contrast = new Animation(0.7, 0, 1);
    [Display(Name = "コントラスト", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Contrast { get => _contrast; set => Set(ref _contrast, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.45, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    Color _tint = Color.FromRgb(12, 12, 14);
    [Display(Name = "線の色", GroupName = "外観")]
    [ColorPicker]
    public Color LineColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<SpeedLinesCustomEffect>(devices, this, d => new SpeedLinesCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Density, Length, Contrast, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Contrast.GetValue(f, len, fps),
            Size = (float)Length.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)Density.GetValue(f, len, fps),
            Mix = 0f,
            Spread = (float)CenterY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = LineColor.R / 255f,
            ColorG = LineColor.G / 255f,
            ColorB = LineColor.B / 255f,
        };
    }
}
