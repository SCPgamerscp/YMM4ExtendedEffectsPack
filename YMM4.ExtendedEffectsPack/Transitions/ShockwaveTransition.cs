using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Transition;
using YMM4.ExtendedEffectsPack.Common;
using YMM4.ExtendedEffectsPack.Effects;

namespace YMM4.ExtendedEffectsPack.Transitions;

public sealed class ShockwaveTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / 衝撃波";
    public ITransitionParameter CreateTransitionParameter() => new ShockwaveTransitionParameter();
}

public sealed class ShockwaveTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

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

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<ShockwaveCustomEffect>(devices, before, after, this, d => new ShockwaveCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Width, Amplitude, Rings, CenterX, CenterY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Amplitude.GetValue(frame, length, fps),
            Size = (float)Width.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)CenterX.GetValue(frame, length, fps),
            Count = (float)Rings.GetValue(frame, length, fps),
            Spread = (float)CenterY.GetValue(frame, length, fps), Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
