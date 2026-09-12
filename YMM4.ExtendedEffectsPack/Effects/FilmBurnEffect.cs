using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum FilmBurnTone
{
    [Display(Name = "ウォームオレンジ")] Warm = 0,
    [Display(Name = "パステルプリズム")] Prism = 1,
    [Display(Name = "クールシアン")] Cool = 2,
}

[VideoEffect("フィルムバーン", new[] { "拡張エフェクト", "演出" }, new[] { "光漏れ" }, IsAviUtlSupported = false)]
public sealed class FilmBurnEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "フィルムバーン";

    Animation _progress = new Animation(42, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _width = new Animation(0.8, 0.1, 2);
    [Display(Name = "感光の広がり", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 2)]
    public Animation Width { get => _width; set => Set(ref _width, value); }

    FilmBurnTone _tone = FilmBurnTone.Warm;
    [Display(Name = "光の色調", GroupName = "基本")]
    [EnumComboBox]
    public FilmBurnTone Tone
    {
        get => _tone;
        set => Set(ref _tone, value);
    }

    bool _grain = true;
    [Display(Name = "フィルム粒子", GroupName = "外観")]
    [ToggleSlider]
    public bool Grain { get => _grain; set => Set(ref _grain, value); }

    bool _burn = true;
    [Display(Name = "焼きつき暗転", GroupName = "外観")]
    [ToggleSlider]
    public bool Burn { get => _burn; set => Set(ref _burn, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<FilmBurnCustomEffect>(devices, this, d => new FilmBurnCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Width];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)Width.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = 0f,
            Mode = (float)(int)Tone,
            FlagA = Grain ? 1f : 0f,
            FlagB = Burn ? 1f : 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = 255/255f,
            ColorG = 160/255f,
            ColorB = 60/255f,
        };
    }
}
