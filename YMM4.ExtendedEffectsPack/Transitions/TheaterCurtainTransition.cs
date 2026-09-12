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

public sealed class TheaterCurtainTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / 劇場カーテン";
    public ITransitionParameter CreateTransitionParameter() => new TheaterCurtainTransitionParameter();
}

public sealed class TheaterCurtainTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.CloseOpen;

    Animation _folds = new Animation(12, 4, 30);
    [Display(Name = "ドレープの数", GroupName = "基本")]
    [AnimationSlider("F2", "", 4, 30)]
    public Animation Folds { get => _folds; set => Set(ref _folds, value); }

    TheaterCurtainOpening _opening = TheaterCurtainOpening.Center;
    [Display(Name = "開き方", GroupName = "基本")]
    [EnumComboBox]
    public TheaterCurtainOpening Opening { get => _opening; set => Set(ref _opening, value); }

    bool _gloss = true;
    [Display(Name = "ベルベット光沢", GroupName = "外観")]
    [ToggleSlider]
    public bool Gloss { get => _gloss; set => Set(ref _gloss, value); }

    bool _fringe = true;
    [Display(Name = "金フリンジ", GroupName = "外観")]
    [ToggleSlider]
    public bool Fringe { get => _fringe; set => Set(ref _fringe, value); }

    Color _tint = Color.FromRgb(138, 11, 26);
    [Display(Name = "カーテン色", GroupName = "外観")]
    [ColorPicker]
    public Color Tint { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<TheaterCurtainCustomEffect>(devices, before, after, this, d => new TheaterCurtainCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Folds];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = 0f, Size = 0f, Speed = 0f, Angle = 0f,
            Count = (float)Folds.GetValue(frame, length, fps),
            Spread = 0f, Mode = (float)(int)Opening,
            FlagA = Gloss ? 1f : 0f, FlagB = Fringe ? 1f : 0f,
            ColorR = Tint.R / 255f, ColorG = Tint.G / 255f, ColorB = Tint.B / 255f,
    };
}
