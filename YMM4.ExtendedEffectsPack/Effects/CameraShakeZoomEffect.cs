using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("ズームブラー＆シェイク", new[] { "拡張エフェクト", "演出" }, new[] { "シェイク" }, IsAviUtlSupported = false)]
public sealed class CameraShakeZoomEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ズームブラー＆シェイク";

    Animation _shake = new Animation(25, 0, 100);
    [Display(Name = "揺れ強度", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 100)]
    public Animation Shake { get => _shake; set => Set(ref _shake, value); }

    Animation _zoomBlur = new Animation(0.4, 0, 1);
    [Display(Name = "放射ブラー", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation ZoomBlur { get => _zoomBlur; set => Set(ref _zoomBlur, value); }

    Animation _hertz = new Animation(24, 1, 60);
    [Display(Name = "振動速度", GroupName = "基本")]
    [AnimationSlider("F2", "Hz", 1, 60)]
    public Animation Hertz { get => _hertz; set => Set(ref _hertz, value); }

    Animation _zoom = new Animation(12, 0, 50);
    [Display(Name = "中心ズーム", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 50)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    Animation _decay = new Animation(0.6, 0.1, 2);
    [Display(Name = "減衰時間", GroupName = "基本")]
    [AnimationSlider("F2", "s", 0.1, 2)]
    public Animation Decay { get => _decay; set => Set(ref _decay, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<CameraShakeZoomCustomEffect>(devices, this, d => new CameraShakeZoomCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Shake, ZoomBlur, Hertz, Zoom, Decay];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Shake.GetValue(f, len, fps),
            Size = (float)Zoom.GetValue(f, len, fps),
            Speed = (float)Hertz.GetValue(f, len, fps),
            Angle = 0f,
            Count = 0f,
            Mix = (float)ZoomBlur.GetValue(f, len, fps),
            Spread = (float)Decay.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)ZoomBlur.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
