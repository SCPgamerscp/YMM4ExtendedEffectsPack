using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum LivingFlamePalette
{
    [Display(Name = "標準（赤黄）")] Fire = 0,
    [Display(Name = "青白い炎")] Blue = 1,
    [Display(Name = "魔界の紫炎")] Purple = 2,
}

[VideoEffect("炎（画像炎化）", new[] { "拡張エフェクト", "加工" }, new[] { "炎" }, IsAviUtlSupported = false)]
public sealed class LivingFlameEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "炎（画像炎化）";

    Animation _distort = new Animation(30, 0, 100);
    [Display(Name = "うねり変形度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 100)]
    public Animation Distort { get => _distort; set => Set(ref _distort, value); }

    Animation _wobble = new Animation(3, 0.1, 10);
    [Display(Name = "揺らぎ速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 10)]
    public Animation Wobble { get => _wobble; set => Set(ref _wobble, value); }

    Animation _burn = new Animation(0, 0, 100);
    [Display(Name = "上部燃焼ディゾルブ", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Burn { get => _burn; set => Set(ref _burn, value); }

    LivingFlamePalette _palette = LivingFlamePalette.Fire;
    [Display(Name = "炎パレット", GroupName = "基本")]
    [EnumComboBox]
    public LivingFlamePalette Palette
    {
        get => _palette;
        set => Set(ref _palette, value);
    }

    bool _colorize = true;
    [Display(Name = "炎カラー着色", GroupName = "外観")]
    [ToggleSlider]
    public bool Colorize { get => _colorize; set => Set(ref _colorize, value); }

    bool _sparks = true;
    [Display(Name = "火の粉", GroupName = "外観")]
    [ToggleSlider]
    public bool Sparks { get => _sparks; set => Set(ref _sparks, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<LivingFlameCustomEffect>(devices, this, d => new LivingFlameCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Distort, Wobble, Burn];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Distort.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)Wobble.GetValue(f, len, fps),
            Angle = 0f,
            Count = 0f,
            Mix = (float)Burn.GetValue(f, len, fps),
            Spread = 0f,
            Mode = (float)(int)Palette,
            FlagA = Colorize ? 1f : 0f,
            FlagB = Sparks ? 1f : 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Burn.GetValue(f, len, fps),
            ColorR = 255/255f,
            ColorG = 120/255f,
            ColorB = 20/255f,
        };
    }
}
