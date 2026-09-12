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

public sealed class PaperTearTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / 紙破り";
    public ITransitionParameter CreateTransitionParameter() => new PaperTearTransitionParameter();
}

public sealed class PaperTearTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.PeakCover;

    Animation _jagged = new Animation(0.65, 0, 1);
    [Display(Name = "ギザギザ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Jagged { get => _jagged; set => Set(ref _jagged, value); }

    Animation _shadow = new Animation(0.5, 0, 1);
    [Display(Name = "裂け目の影", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Shadow { get => _shadow; set => Set(ref _shadow, value); }

    PaperTearDirection _direction = PaperTearDirection.Horizontal;
    [Display(Name = "方向", GroupName = "基本")]
    [EnumComboBox]
    public PaperTearDirection Direction { get => _direction; set => Set(ref _direction, value); }

    Color _tint = Color.FromRgb(230, 223, 210);
    [Display(Name = "裏面の色", GroupName = "外観")]
    [ColorPicker]
    public Color BackColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<PaperTearCustomEffect>(devices, before, after, this, d => new PaperTearCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Jagged, Shadow];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Shadow.GetValue(frame, length, fps), Size = 0f, Speed = 0f, Angle = 0f,
            Count = 0f, Spread = (float)Jagged.GetValue(frame, length, fps),
            Mode = (float)(int)Direction, FlagA = 0f, FlagB = 0f,
            ColorR = BackColor.R / 255f, ColorG = BackColor.G / 255f, ColorB = BackColor.B / 255f,
    };
}
