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

public sealed class IrisWipeTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / アイリスワイプ";
    public ITransitionParameter CreateTransitionParameter() => new IrisWipeTransitionParameter();
}

public sealed class IrisWipeTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.CloseOpen;

    Animation _sides = new Animation(6, 3, 12);
    [Display(Name = "辺の数", GroupName = "基本")]
    [AnimationSlider("F2", "", 3, 12)]
    public Animation Sides { get => _sides; set => Set(ref _sides, value); }

    Animation _softness = new Animation(0.06, 0, 0.2);
    [Display(Name = "縁のぼかし", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.2)]
    public Animation Softness { get => _softness; set => Set(ref _softness, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    IrisWipeShape _shape = IrisWipeShape.Circle;
    [Display(Name = "形状", GroupName = "基本")]
    [EnumComboBox]
    public IrisWipeShape Shape { get => _shape; set => Set(ref _shape, value); }

    Color _tint = Color.FromRgb(5, 5, 6);
    [Display(Name = "マスク色", GroupName = "外観")]
    [ColorPicker]
    public Color MaskColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<IrisWipeCustomEffect>(devices, before, after, this, d => new IrisWipeCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Sides, Softness, CenterX, CenterY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = 0f, Size = (float)Softness.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)CenterX.GetValue(frame, length, fps),
            Count = (float)Sides.GetValue(frame, length, fps),
            Spread = (float)CenterY.GetValue(frame, length, fps),
            Mode = (float)(int)Shape, FlagA = 0f, FlagB = 0f,
            ColorR = MaskColor.R / 255f, ColorG = MaskColor.G / 255f, ColorB = MaskColor.B / 255f,
    };
}
