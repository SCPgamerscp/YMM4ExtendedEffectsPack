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

public sealed class PrismWhipTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / プリズムウィップ";
    public ITransitionParameter CreateTransitionParameter() => new PrismWhipTransitionParameter();
}

public sealed class PrismWhipTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

    Animation _angle = new Animation(0, 0, 360);
    [Display(Name = "パン角度", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 360)]
    public Animation Angle { get => _angle; set => Set(ref _angle, value); }

    Animation _blur = new Animation(0.5, 0, 1);
    [Display(Name = "方向ブラー", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Blur { get => _blur; set => Set(ref _blur, value); }

    Animation _chromatic = new Animation(12, 0, 40);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 40)]
    public Animation Chromatic { get => _chromatic; set => Set(ref _chromatic, value); }

    Animation _zoom = new Animation(15, 0, 50);
    [Display(Name = "ズーム", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 50)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<PrismWhipCustomEffect>(devices, before, after, this, d => new PrismWhipCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Angle, Blur, Chromatic, Zoom];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Blur.GetValue(frame, length, fps),
            Size = (float)Zoom.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)Angle.GetValue(frame, length, fps), Count = 0f,
            Spread = (float)Chromatic.GetValue(frame, length, fps), Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
