using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("古フィルム", new[] { "拡張エフェクト", "加工" }, new[] { "映写機" }, IsAviUtlSupported = false)]
public sealed class OldFilmEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "古フィルム";

    Animation _dirt = new Animation(0.45, 0, 1);
    [Display(Name = "傷・塵", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Dirt { get => _dirt; set => Set(ref _dirt, value); }

    Animation _flicker = new Animation(0.3, 0, 1);
    [Display(Name = "点滅", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Flicker { get => _flicker; set => Set(ref _flicker, value); }

    Animation _gateShake = new Animation(1.2, 0, 4);
    [Display(Name = "ゲート揺れ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 4)]
    public Animation GateShake { get => _gateShake; set => Set(ref _gateShake, value); }

    Animation _sepia = new Animation(0.55, 0, 1);
    [Display(Name = "セピア", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Sepia { get => _sepia; set => Set(ref _sepia, value); }

    Animation _vignette = new Animation(0.45, 0, 1);
    [Display(Name = "ビネット", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Vignette { get => _vignette; set => Set(ref _vignette, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<OldFilmCustomEffect>(devices, this, d => new OldFilmCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Dirt, Flicker, GateShake, Sepia, Vignette];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Dirt.GetValue(f, len, fps),
            Size = (float)Sepia.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = (float)Vignette.GetValue(f, len, fps),
            Mix = (float)Flicker.GetValue(f, len, fps),
            Spread = (float)GateShake.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Flicker.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
