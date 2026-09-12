using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum HalftoneStyle
{
    [Display(Name = "モノクロ網点")] Mono = 0,
    [Display(Name = "CMYK 4色")] Cmyk = 1,
}

[VideoEffect("ハーフトーン", new[] { "拡張エフェクト", "加工" }, new[] { "網点" }, IsAviUtlSupported = false)]
public sealed class HalftoneEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ハーフトーン";

    Animation _dotSize = new Animation(8, 3, 24);
    [Display(Name = "網点サイズ", GroupName = "基本")]
    [AnimationSlider("F2", "", 3, 24)]
    public Animation DotSize { get => _dotSize; set => Set(ref _dotSize, value); }

    Animation _contrast = new Animation(0.65, 0, 1);
    [Display(Name = "コントラスト", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Contrast { get => _contrast; set => Set(ref _contrast, value); }

    Animation _screenAngle = new Animation(22, 0, 90);
    [Display(Name = "スクリーン角", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 90)]
    public Animation ScreenAngle { get => _screenAngle; set => Set(ref _screenAngle, value); }

    HalftoneStyle _style = HalftoneStyle.Mono;
    [Display(Name = "様式", GroupName = "基本")]
    [EnumComboBox]
    public HalftoneStyle Style
    {
        get => _style;
        set => Set(ref _style, value);
    }

    bool _paper = true;
    [Display(Name = "紙の地色", GroupName = "外観")]
    [ToggleSlider]
    public bool Paper { get => _paper; set => Set(ref _paper, value); }

    Color _tint = Color.FromRgb(17, 17, 20);
    [Display(Name = "インク色", GroupName = "外観")]
    [ColorPicker]
    public Color InkColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<HalftoneCustomEffect>(devices, this, d => new HalftoneCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [DotSize, Contrast, ScreenAngle];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Contrast.GetValue(f, len, fps),
            Size = (float)DotSize.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)ScreenAngle.GetValue(f, len, fps),
            Count = 0f,
            Mix = 0f,
            Spread = 0f,
            Mode = (float)(int)Style,
            FlagA = Paper ? 1f : 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = InkColor.R / 255f,
            ColorG = InkColor.G / 255f,
            ColorB = InkColor.B / 255f,
        };
    }
}
