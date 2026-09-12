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

public sealed class LumaMeltTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / ルマメルト";
    public ITransitionParameter CreateTransitionParameter() => new LumaMeltTransitionParameter();
}

public sealed class LumaMeltTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.PeakCover;

    Animation _amount = new Animation(0.7, 0, 1);
    [Display(Name = "溶ける量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Amount { get => _amount; set => Set(ref _amount, value); }

    Animation _turbulence = new Animation(0.55, 0, 1);
    [Display(Name = "液の乱れ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Turbulence { get => _turbulence; set => Set(ref _turbulence, value); }

    LumaMeltDirection _direction = LumaMeltDirection.Down;
    [Display(Name = "方向", GroupName = "基本")]
    [EnumComboBox]
    public LumaMeltDirection Direction { get => _direction; set => Set(ref _direction, value); }

    Color _tint = Color.FromRgb(26, 14, 8);
    [Display(Name = "溶け跡の色", GroupName = "外観")]
    [ColorPicker]
    public Color MeltColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<LumaMeltCustomEffect>(devices, before, after, this, d => new LumaMeltCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Turbulence];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Amount.GetValue(frame, length, fps), Size = 0f, Speed = 0f, Angle = 0f,
            Count = 0f, Spread = (float)Turbulence.GetValue(frame, length, fps),
            Mode = (float)(int)Direction, FlagA = 0f, FlagB = 0f,
            ColorR = MeltColor.R / 255f, ColorG = MeltColor.G / 255f, ColorB = MeltColor.B / 255f,
    };
}
