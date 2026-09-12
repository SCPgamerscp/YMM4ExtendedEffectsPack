using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum GamingRainbowGlowMode
{
    [Display(Name = "全体色相回転")] Hue = 0,
    [Display(Name = "流動光波ウェーブ")] Wave = 1,
    [Display(Name = "輪郭ネオン発光")] Edge = 2,
}

public enum GamingRainbowBlend
{
    [Display(Name = "通常上書き")] Normal = 0,
    [Display(Name = "加算（爆光）")] Add = 1,
    [Display(Name = "スクリーン")] Screen = 2,
    [Display(Name = "色相置換")] HueBlend = 3,
}

[VideoEffect("七色ゲーミング", new[] { "拡張エフェクト", "加工" }, new[] { "RGB", "ネオン" }, IsAviUtlSupported = false)]
public sealed class GamingRainbowEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "七色ゲーミング";

    Animation _hueSpeed = new Animation(5, 0, 50);
    [Display(Name = "回転速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 50)]
    public Animation HueSpeed { get => _hueSpeed; set => Set(ref _hueSpeed, value); }

    Animation _waveAngle = new Animation(45, 0, 360);
    [Display(Name = "波の角度", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 360)]
    public Animation WaveAngle { get => _waveAngle; set => Set(ref _waveAngle, value); }

    Animation _saturation = new Animation(2, 1, 5);
    [Display(Name = "彩度・発光", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 5)]
    public Animation Saturation { get => _saturation; set => Set(ref _saturation, value); }

    GamingRainbowGlowMode _glowMode = GamingRainbowGlowMode.Hue;
    [Display(Name = "発光モード", GroupName = "基本")]
    [EnumComboBox]
    public GamingRainbowGlowMode GlowMode
    {
        get => _glowMode;
        set => Set(ref _glowMode, value);
    }

    GamingRainbowBlend _blend = GamingRainbowBlend.Normal;
    [Display(Name = "合成モード", GroupName = "基本")]
    [EnumComboBox]
    public GamingRainbowBlend Blend
    {
        get => _blend;
        set => Set(ref _blend, value);
    }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<GamingRainbowCustomEffect>(devices, this, d => new GamingRainbowCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [HueSpeed, WaveAngle, Saturation];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Saturation.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)HueSpeed.GetValue(f, len, fps),
            Angle = (float)WaveAngle.GetValue(f, len, fps),
            Count = 0f,
            Mix = 0f,
            Spread = (float)(int)Blend,
            Mode = (float)(int)GlowMode,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = 0/255f,
            ColorG = 255/255f,
            ColorB = 180/255f,
        };
    }
}
