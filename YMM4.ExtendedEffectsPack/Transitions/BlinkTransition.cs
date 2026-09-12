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

public sealed class BlinkTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / まばたき";
    public ITransitionParameter CreateTransitionParameter() => new BlinkTransitionParameter();
}

public sealed class BlinkTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.CloseOpen;

    Animation _blur = new Animation(10, 0, 20);
    [Display(Name = "ピンボケ強度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 20)]
    public Animation Blur { get => _blur; set => Set(ref _blur, value); }

    Color _tint = Color.FromRgb(0, 0, 0);
    [Display(Name = "まぶたの色", GroupName = "外観")]
    [ColorPicker]
    public Color LidColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<BlinkCustomEffect>(devices, before, after, this, d => new BlinkCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Blur];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Blur.GetValue(frame, length, fps), Size = 0f, Speed = 18f, Angle = 0f,
            Count = 0f, Spread = 0f, Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = LidColor.R / 255f, ColorG = LidColor.G / 255f, ColorB = LidColor.B / 255f,
    };
}
