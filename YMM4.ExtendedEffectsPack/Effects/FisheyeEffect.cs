using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("魚眼レンズ", new[] { "拡張エフェクト", "加工" }, new[] { "レンズ" }, IsAviUtlSupported = false)]
public sealed class FisheyeEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "魚眼レンズ";

    Animation _k = new Animation(0.42, -0.4, 0.9);
    [Display(Name = "歪み強度", GroupName = "基本")]
    [AnimationSlider("F2", "", -0.4, 0.9)]
    public Animation K { get => _k; set => Set(ref _k, value); }

    Animation _chroma = new Animation(0.012, 0, 0.05);
    [Display(Name = "周辺色収差", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.05)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    Animation _vignette = new Animation(0.35, 0, 1);
    [Display(Name = "周辺減光", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Vignette { get => _vignette; set => Set(ref _vignette, value); }

    Animation _zoom = new Animation(1, 0.7, 1.4);
    [Display(Name = "ズーム補正", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.7, 1.4)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<FisheyeCustomEffect>(devices, this, d => new FisheyeCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [K, Chroma, Vignette, Zoom];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)K.GetValue(f, len, fps),
            Size = (float)Zoom.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Vignette.GetValue(f, len, fps),
            Spread = (float)Chroma.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Vignette.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
