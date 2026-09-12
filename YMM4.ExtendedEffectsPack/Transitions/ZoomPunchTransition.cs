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

public sealed class ZoomPunchTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / ズームパンチ";
    public ITransitionParameter CreateTransitionParameter() => new ZoomPunchTransitionParameter();
}

public sealed class ZoomPunchTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

    Animation _zoom = new Animation(0.5, 0, 1);
    [Display(Name = "ズーム量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    Animation _hit = new Animation(0.7, 0.2, 1);
    [Display(Name = "パンチの鋭さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 1)]
    public Animation Hit { get => _hit; set => Set(ref _hit, value); }

    Animation _chroma = new Animation(0.4, 0, 1);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.45, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<ZoomPunchCustomEffect>(devices, before, after, this, d => new ZoomPunchCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Zoom, Hit, Chroma, CenterX, CenterY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Hit.GetValue(frame, length, fps),
            Size = (float)Zoom.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)CenterX.GetValue(frame, length, fps),
            Count = (float)CenterY.GetValue(frame, length, fps),
            Spread = (float)Chroma.GetValue(frame, length, fps), Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
