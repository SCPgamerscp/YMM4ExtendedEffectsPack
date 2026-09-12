using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("モーションゴースト", new[] { "拡張エフェクト", "加工" }, new[] { "残像" }, IsAviUtlSupported = false)]
public sealed class MotionGhostEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "モーションゴースト";

    Animation _delay = new Animation(6, 1, 30);
    [Display(Name = "残像の長さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 30)]
    public Animation Delay { get => _delay; set => Set(ref _delay, value); }

    Animation _opacity = new Animation(0.5, 0, 1);
    [Display(Name = "ゴースト不透明度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Opacity { get => _opacity; set => Set(ref _opacity, value); }

    Animation _split = new Animation(15, 0, 50);
    [Display(Name = "RGB分離", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 50)]
    public Animation Split { get => _split; set => Set(ref _split, value); }

    Animation _echo = new Animation(8, 0, 30);
    [Display(Name = "拡大エコー", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 30)]
    public Animation Echo { get => _echo; set => Set(ref _echo, value); }

    Animation _fade = new Animation(2.4, 1, 5);
    [Display(Name = "フェード速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 1, 5)]
    public Animation Fade { get => _fade; set => Set(ref _fade, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new MotionGhostProcessor(devices, this);
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Delay, Opacity, Split, Echo, Fade];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)Echo.GetValue(f, len, fps),
            Speed = (float)Fade.GetValue(f, len, fps),
            Angle = 0f,
            Count = (float)Delay.GetValue(f, len, fps),
            Mix = (float)Opacity.GetValue(f, len, fps),
            Spread = (float)Split.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Opacity.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
