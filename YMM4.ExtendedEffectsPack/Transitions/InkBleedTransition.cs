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

public sealed class InkBleedTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / インクスプラッシュ";
    public ITransitionParameter CreateTransitionParameter() => new InkBleedTransitionParameter();
}

public sealed class InkBleedTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.PeakCover;

    Animation _spreadAmt = new Animation(0.55, 0, 1);
    [Display(Name = "にじみ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation SpreadAmt { get => _spreadAmt; set => Set(ref _spreadAmt, value); }

    Animation _veins = new Animation(0.65, 0, 1);
    [Display(Name = "葉脈の複雑さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Veins { get => _veins; set => Set(ref _veins, value); }

    Animation _originX = new Animation(0.5, 0, 1);
    [Display(Name = "発生点 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OriginX { get => _originX; set => Set(ref _originX, value); }

    Animation _originY = new Animation(0.5, 0, 1);
    [Display(Name = "発生点 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OriginY { get => _originY; set => Set(ref _originY, value); }

    Color _tint = Color.FromRgb(17, 17, 20);
    [Display(Name = "インク色", GroupName = "外観")]
    [ColorPicker]
    public Color InkColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<InkBleedCustomEffect>(devices, before, after, this, d => new InkBleedCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [SpreadAmt, Veins, OriginX, OriginY];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = 0f, Size = (float)OriginY.GetValue(frame, length, fps), Speed = 0f,
            Angle = (float)OriginX.GetValue(frame, length, fps),
            Count = (float)Veins.GetValue(frame, length, fps),
            Spread = (float)SpreadAmt.GetValue(frame, length, fps), Mode = 0f, FlagA = 0f, FlagB = 0f,
            ColorR = InkColor.R / 255f, ColorG = InkColor.G / 255f, ColorB = InkColor.B / 255f,
    };
}
