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

public sealed class MatrixRainTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / マトリックス";
    public ITransitionParameter CreateTransitionParameter() => new MatrixRainTransitionParameter();
}

public sealed class MatrixRainTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

    Animation _fallSpeed = new Animation(2.5, 0.5, 10);
    [Display(Name = "落下速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.5, 10)]
    public Animation FallSpeed { get => _fallSpeed; set => Set(ref _fallSpeed, value); }

    Animation _glyphSize = new Animation(18, 8, 48);
    [Display(Name = "文字サイズ", GroupName = "基本")]
    [AnimationSlider("F2", "px", 8, 48)]
    public Animation GlyphSize { get => _glyphSize; set => Set(ref _glyphSize, value); }

    Animation _trail = new Animation(14, 5, 30);
    [Display(Name = "残光の長さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 5, 30)]
    public Animation Trail { get => _trail; set => Set(ref _trail, value); }

    MatrixRainCompose _compose = MatrixRainCompose.Overlay;
    [Display(Name = "合成モード", GroupName = "基本")]
    [EnumComboBox]
    public MatrixRainCompose Compose { get => _compose; set => Set(ref _compose, value); }

    Color _tint = Color.FromRgb(0, 255, 65);
    [Display(Name = "文字カラー", GroupName = "外観")]
    [ColorPicker]
    public Color GlyphColor { get => _tint; set => Set(ref _tint, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<MatrixRainCustomEffect>(devices, before, after, this, d => new MatrixRainCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [FallSpeed, GlyphSize, Trail];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = 0f, Size = (float)GlyphSize.GetValue(frame, length, fps),
            Speed = (float)FallSpeed.GetValue(frame, length, fps), Angle = 0f,
            Count = (float)Trail.GetValue(frame, length, fps), Spread = 0f,
            Mode = (float)(int)Compose, FlagA = 0f, FlagB = 0f,
            ColorR = GlyphColor.R / 255f, ColorG = GlyphColor.G / 255f, ColorB = GlyphColor.B / 255f,
    };
}
