using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum MatrixRainCompose
{
    [Display(Name = "背景オーバーレイ")] Overlay = 0,
    [Display(Name = "元画像コード分解")] Ascii = 1,
    [Display(Name = "01バイナリ")] Binary = 2,
}

[VideoEffect("マトリックス", new[] { "拡張エフェクト", "加工" }, new[] { "コード" }, IsAviUtlSupported = false)]
public sealed class MatrixRainEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "マトリックス";

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
    public MatrixRainCompose Compose
    {
        get => _compose;
        set => Set(ref _compose, value);
    }

    Color _tint = Color.FromRgb(0, 255, 65);
    [Display(Name = "文字カラー", GroupName = "外観")]
    [ColorPicker]
    public Color GlyphColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<MatrixRainCustomEffect>(devices, this, d => new MatrixRainCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [FallSpeed, GlyphSize, Trail];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)GlyphSize.GetValue(f, len, fps),
            Speed = (float)FallSpeed.GetValue(f, len, fps),
            Angle = 0f,
            Count = (float)Trail.GetValue(f, len, fps),
            Mix = 0f,
            Spread = 0f,
            Mode = (float)(int)Compose,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = GlyphColor.R / 255f,
            ColorG = GlyphColor.G / 255f,
            ColorB = GlyphColor.B / 255f,
        };
    }
}
