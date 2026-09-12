using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("水面リップル", new[] { "拡張エフェクト", "加工" }, new[] { "水面" }, IsAviUtlSupported = false)]
public sealed class WaterRippleEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "水面リップル";

    Animation _amplitude = new Animation(0.03, 0, 0.1);
    [Display(Name = "振幅", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.1)]
    public Animation Amplitude { get => _amplitude; set => Set(ref _amplitude, value); }

    Animation _frequency = new Animation(22, 4, 50);
    [Display(Name = "周波数", GroupName = "基本")]
    [AnimationSlider("F2", "", 4, 50)]
    public Animation Frequency { get => _frequency; set => Set(ref _frequency, value); }

    Animation _rippleSpeed = new Animation(2.8, 0.2, 8);
    [Display(Name = "速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 8)]
    public Animation RippleSpeed { get => _rippleSpeed; set => Set(ref _rippleSpeed, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.55, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    Animation _wet = new Animation(0.35, 0, 1);
    [Display(Name = "濡れハイライト", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Wet { get => _wet; set => Set(ref _wet, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<WaterRippleCustomEffect>(devices, this, d => new WaterRippleCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amplitude, Frequency, RippleSpeed, CenterX, CenterY, Wet];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amplitude.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)RippleSpeed.GetValue(f, len, fps),
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)Frequency.GetValue(f, len, fps),
            Mix = (float)Wet.GetValue(f, len, fps),
            Spread = (float)CenterY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Wet.GetValue(f, len, fps),
            ColorR = 180/255f,
            ColorG = 220/255f,
            ColorB = 255/255f,
        };
    }
}
