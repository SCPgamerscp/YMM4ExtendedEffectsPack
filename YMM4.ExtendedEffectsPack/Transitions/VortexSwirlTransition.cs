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

public sealed class VortexSwirlTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / 渦巻き";
    public ITransitionParameter CreateTransitionParameter() => new VortexSwirlTransitionParameter();
}

public sealed class VortexSwirlTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.PeakCover;

    Animation _turns = new Animation(2.2, 0.2, 6);
    [Display(Name = "回転量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 6)]
    public Animation Turns { get => _turns; set => Set(ref _turns, value); }

    Animation _falloff = new Animation(0.55, 0, 1);
    [Display(Name = "中心の残り", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Falloff { get => _falloff; set => Set(ref _falloff, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    bool _chroma = true;
    [Display(Name = "色収差", GroupName = "外観")]
    [ToggleSlider]
    public bool Chroma { get => _chroma; set => Set(ref _chroma, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<VortexSwirlCustomEffect>(devices, before, after, this, d => new VortexSwirlCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Turns, Falloff, CenterX, CenterY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Turns.GetValue(frame, length, fps),
            Size = (float)Falloff.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)CenterX.GetValue(frame, length, fps), Count = 0f,
            Spread = (float)CenterY.GetValue(frame, length, fps), Mode = 0f,
            FlagA = Chroma ? 1f : 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
