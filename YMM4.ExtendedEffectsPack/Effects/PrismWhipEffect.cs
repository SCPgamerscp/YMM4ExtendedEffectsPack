using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

[VideoEffect("プリズムウィップ", new[] { "拡張エフェクト", "演出" }, new[] { "プリズム", "ウィップ" }, IsAviUtlSupported = false)]
public sealed class PrismWhipEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "プリズムウィップ";

    Animation _progress = new Animation(0, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _angle = new Animation(0, 0, 360);
    [Display(Name = "パン角度", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 360)]
    public Animation Angle { get => _angle; set => Set(ref _angle, value); }

    Animation _blur = new Animation(0.5, 0, 1);
    [Display(Name = "方向ブラー", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Blur { get => _blur; set => Set(ref _blur, value); }

    Animation _chromatic = new Animation(12, 0, 40);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 40)]
    public Animation Chromatic { get => _chromatic; set => Set(ref _chromatic, value); }

    Animation _zoom = new Animation(15, 0, 50);
    [Display(Name = "ズーム", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 50)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<PrismWhipCustomEffect>(devices, this, d => new PrismWhipCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Angle, Blur, Chromatic, Zoom];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Blur.GetValue(f, len, fps),
            Size = (float)Zoom.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)Angle.GetValue(f, len, fps),
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Chromatic.GetValue(f, len, fps),
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
