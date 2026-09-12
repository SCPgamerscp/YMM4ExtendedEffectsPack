using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum GlitchShiftStyle
{
    [Display(Name = "RGBスプリット")] Rgb = 0,
    [Display(Name = "ブロックずれ")] Block = 1,
    [Display(Name = "スライス")] Slice = 2,
}

[VideoEffect("グリッチシフト", new[] { "拡張エフェクト", "演出" }, new[] { "グリッチ" }, IsAviUtlSupported = false)]
public sealed class GlitchShiftEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "グリッチシフト";

    Animation _progress = new Animation(48, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

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
    public GlitchShiftStyle Style
    {
        get => _style;
        set => Set(ref _style, value);
    }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<GlitchShiftCustomEffect>(devices, this, d => new GlitchShiftCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Blocks, Split, NoiseAmt];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)NoiseAmt.GetValue(f, len, fps),
            Size = 0f,
            Speed = 0f,
            Angle = 0f,
            Count = (float)Blocks.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Split.GetValue(f, len, fps),
            Mode = (float)(int)Style,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
