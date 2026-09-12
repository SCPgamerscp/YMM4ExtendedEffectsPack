using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("衝撃波", new[] { "拡張エフェクト", "演出" }, new[] { "衝撃" }, IsAviUtlSupported = false)]
public sealed class ShockwaveEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "衝撃波";

    Animation _progress = new Animation(40, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _width = new Animation(0.08, 0.02, 0.25);
    [Display(Name = "リング幅", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.02, 0.25)]
    public Animation Width { get => _width; set => Set(ref _width, value); }

    Animation _amplitude = new Animation(0.12, 0, 0.35);
    [Display(Name = "歪み", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.35)]
    public Animation Amplitude { get => _amplitude; set => Set(ref _amplitude, value); }

    Animation _rings = new Animation(1, 1, 4);
    [Display(Name = "リング数", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 4)]
    public Animation Rings { get => _rings; set => Set(ref _rings, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<ShockwaveCustomEffect>(devices, this, d => new ShockwaveCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Width, Amplitude, Rings, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amplitude.GetValue(f, len, fps),
            Size = (float)Width.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)Rings.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)CenterY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
