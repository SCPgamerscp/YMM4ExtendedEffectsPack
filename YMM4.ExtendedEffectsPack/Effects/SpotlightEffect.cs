using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("スポットライト", new[] { "拡張エフェクト", "描画" }, new[] { "照明" }, IsAviUtlSupported = false)]
public sealed class SpotlightEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "スポットライト";

    Animation _progress = new Animation(70, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _radius = new Animation(0.3, 0.05, 0.8);
    [Display(Name = "半径", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.05, 0.8)]
    public Animation Radius { get => _radius; set => Set(ref _radius, value); }

    Animation _softness = new Animation(0.2, 0.02, 0.5);
    [Display(Name = "縁の柔らかさ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.02, 0.5)]
    public Animation Softness { get => _softness; set => Set(ref _softness, value); }

    Animation _gain = new Animation(1.15, 0.4, 2);
    [Display(Name = "光量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.4, 2)]
    public Animation Gain { get => _gain; set => Set(ref _gain, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.4, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    Color _tint = Color.FromRgb(255, 243, 208);
    [Display(Name = "ライト色", GroupName = "外観")]
    [ColorPicker]
    public Color LightColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<SpotlightCustomEffect>(devices, this, d => new SpotlightCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Radius, Softness, Gain, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Gain.GetValue(f, len, fps),
            Size = (float)Radius.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)CenterY.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Softness.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = LightColor.R / 255f,
            ColorG = LightColor.G / 255f,
            ColorB = LightColor.B / 255f,
        };
    }
}
