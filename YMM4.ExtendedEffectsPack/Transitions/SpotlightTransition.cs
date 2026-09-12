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

public sealed class SpotlightTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / スポットライト";
    public ITransitionParameter CreateTransitionParameter() => new SpotlightTransitionParameter();
}

public sealed class SpotlightTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.CloseOpen;

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

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<SpotlightCustomEffect>(devices, before, after, this, d => new SpotlightCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Radius, Softness, Gain, CenterX, CenterY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Gain.GetValue(frame, length, fps),
            Size = (float)Radius.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)CenterX.GetValue(frame, length, fps),
            Count = (float)CenterY.GetValue(frame, length, fps),
            Spread = (float)Softness.GetValue(frame, length, fps), Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = LightColor.R / 255f, ColorG = LightColor.G / 255f, ColorB = LightColor.B / 255f,
    };
}
