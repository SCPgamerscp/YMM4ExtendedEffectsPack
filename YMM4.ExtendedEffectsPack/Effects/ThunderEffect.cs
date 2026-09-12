using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum ThunderThunderMode
{
    [Display(Name = "落雷フラッシュ")] Strike = 0,
    [Display(Name = "輪郭帯電オーラ")] Aura = 1,
}

[VideoEffect("サンダー", new[] { "拡張エフェクト", "描画" }, new[] { "雷" }, IsAviUtlSupported = false)]
public sealed class ThunderEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "サンダー";

    Animation _frequency = new Animation(12, 1, 30);
    [Display(Name = "バチバチ頻度", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 30)]
    public Animation Frequency { get => _frequency; set => Set(ref _frequency, value); }

    Animation _thickness = new Animation(4, 1, 20);
    [Display(Name = "稲妻の太さ", GroupName = "基本")]
    [AnimationSlider("F2", "px", 1, 20)]
    public Animation Thickness { get => _thickness; set => Set(ref _thickness, value); }

    Animation _jagged = new Animation(6, 1, 10);
    [Display(Name = "ギザギザ・分岐", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 10)]
    public Animation Jagged { get => _jagged; set => Set(ref _jagged, value); }

    Animation _flash = new Animation(0.7, 0, 1);
    [Display(Name = "画面閃光", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Flash { get => _flash; set => Set(ref _flash, value); }

    ThunderThunderMode _thunderMode = ThunderThunderMode.Strike;
    [Display(Name = "動作モード", GroupName = "基本")]
    [EnumComboBox]
    public ThunderThunderMode ThunderMode
    {
        get => _thunderMode;
        set => Set(ref _thunderMode, value);
    }

    Color _tint = Color.FromRgb(176, 226, 255);
    [Display(Name = "電撃カラー", GroupName = "外観")]
    [ColorPicker]
    public Color BoltColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<ThunderCustomEffect>(devices, this, d => new ThunderCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Frequency, Thickness, Jagged, Flash];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)Thickness.GetValue(f, len, fps),
            Speed = (float)Frequency.GetValue(f, len, fps),
            Angle = 0f,
            Count = (float)Jagged.GetValue(f, len, fps),
            Mix = (float)Flash.GetValue(f, len, fps),
            Spread = 0f,
            Mode = (float)(int)ThunderMode,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Flash.GetValue(f, len, fps),
            ColorR = BoltColor.R / 255f,
            ColorG = BoltColor.G / 255f,
            ColorB = BoltColor.B / 255f,
        };
    }
}
