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

public sealed class WhipPanTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / ホイップパン";
    public ITransitionParameter CreateTransitionParameter() => new WhipPanTransitionParameter();
}

public sealed class WhipPanTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

    Animation _direction = new Animation(0, 0, 180);
    [Display(Name = "方向", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 180)]
    public Animation Direction { get => _direction; set => Set(ref _direction, value); }

    Animation _smear = new Animation(0.85, 0, 1.5);
    [Display(Name = "ブラー量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1.5)]
    public Animation Smear { get => _smear; set => Set(ref _smear, value); }

    Animation _chroma = new Animation(0.4, 0, 1);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    bool _blackout = true;
    [Display(Name = "中間ブラックアウト", GroupName = "外観")]
    [ToggleSlider]
    public bool Blackout { get => _blackout; set => Set(ref _blackout, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<WhipPanCustomEffect>(devices, before, after, this, d => new WhipPanCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Direction, Smear, Chroma];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Smear.GetValue(frame, length, fps), Size = 0f, Speed = 0f,
            Angle = (float)Direction.GetValue(frame, length, fps), Count = 0f,
            Spread = (float)Chroma.GetValue(frame, length, fps), Mode = 0f,
            FlagA = Blackout ? 1f : 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
