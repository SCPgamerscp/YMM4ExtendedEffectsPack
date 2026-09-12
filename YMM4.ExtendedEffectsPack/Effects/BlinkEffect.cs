using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("まばたき", new[] { "拡張エフェクト", "演出" }, new[] { "まぶた" }, IsAviUtlSupported = false)]
public sealed class BlinkEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "まばたき";

    Animation _openAmount = new Animation(72, 0, 100);
    [Display(Name = "開き具合", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation OpenAmount { get => _openAmount; set => Set(ref _openAmount, value); }

    Animation _rate = new Animation(18, 5, 60);
    [Display(Name = "まばたき頻度", GroupName = "基本")]
    [AnimationSlider("F2", "回/分", 5, 60)]
    public Animation Rate { get => _rate; set => Set(ref _rate, value); }

    Animation _blur = new Animation(10, 0, 20);
    [Display(Name = "ピンボケ強度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 20)]
    public Animation Blur { get => _blur; set => Set(ref _blur, value); }

    bool _autoBlink = false;
    [Display(Name = "自動ループ", GroupName = "外観")]
    [ToggleSlider]
    public bool AutoBlink { get => _autoBlink; set => Set(ref _autoBlink, value); }

    Color _tint = Color.FromRgb(0, 0, 0);
    [Display(Name = "まぶたの色", GroupName = "外観")]
    [ColorPicker]
    public Color LidColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<BlinkCustomEffect>(devices, this, d => new BlinkCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [OpenAmount, Rate, Blur];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Blur.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)Rate.GetValue(f, len, fps),
            Angle = 0f,
            Count = 0f,
            Mix = (float)OpenAmount.GetValue(f, len, fps),
            Spread = 0f,
            Mode = 0f,
            FlagA = AutoBlink ? 1f : 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)OpenAmount.GetValue(f, len, fps),
            ColorR = LidColor.R / 255f,
            ColorG = LidColor.G / 255f,
            ColorB = LidColor.B / 255f,
        };
    }
}
