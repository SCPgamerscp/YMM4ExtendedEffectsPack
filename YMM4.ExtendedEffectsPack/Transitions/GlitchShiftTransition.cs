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

public sealed class GlitchShiftTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / グリッチシフト";
    public ITransitionParameter CreateTransitionParameter() => new GlitchShiftTransitionParameter();
}

public sealed class GlitchShiftTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

    Animation _blocks = new Animation(18, 4, 48);
    [Display(Name = "ブロック分割", GroupName = "基本")]
    [AnimationSlider("F2", "", 4, 48)]
    public Animation Blocks { get => _blocks; set => Set(ref _blocks, value); }

    Animation _split = new Animation(0.65, 0, 1);
    [Display(Name = "RGB分離", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Split { get => _split; set => Set(ref _split, value); }

    Animation _noiseAmt = new Animation(0.35, 0, 1);
    [Display(Name = "ノイズ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation NoiseAmt { get => _noiseAmt; set => Set(ref _noiseAmt, value); }

    GlitchShiftStyle _style = GlitchShiftStyle.Rgb;
    [Display(Name = "スタイル", GroupName = "基本")]
    [EnumComboBox]
    public GlitchShiftStyle Style { get => _style; set => Set(ref _style, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<GlitchShiftCustomEffect>(devices, before, after, this, d => new GlitchShiftCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Blocks, Split, NoiseAmt];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)NoiseAmt.GetValue(frame, length, fps), Size = 0f, Speed = 0f, Angle = 0f,
            Count = (float)Blocks.GetValue(frame, length, fps),
            Spread = (float)Split.GetValue(frame, length, fps),
            Mode = (float)(int)Style, FlagA = 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
