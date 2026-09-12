using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("ズームパンチ", new[] { "拡張エフェクト", "演出" }, new[] { "パンチ" }, IsAviUtlSupported = false)]
public sealed class ZoomPunchEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ズームパンチ";

    Animation _progress = new Animation(38, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _zoom = new Animation(0.5, 0, 1);
    [Display(Name = "ズーム量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    Animation _hit = new Animation(0.7, 0.2, 1);
    [Display(Name = "パンチの鋭さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 1)]
    public Animation Hit { get => _hit; set => Set(ref _hit, value); }

    Animation _chroma = new Animation(0.4, 0, 1);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.45, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<ZoomPunchCustomEffect>(devices, this, d => new ZoomPunchCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Zoom, Hit, Chroma, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Hit.GetValue(f, len, fps),
            Size = (float)Zoom.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)CenterY.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Chroma.GetValue(f, len, fps),
            Mode = 0f,
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
