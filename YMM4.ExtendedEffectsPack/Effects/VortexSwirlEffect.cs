using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("渦巻き", new[] { "拡張エフェクト", "演出" }, new[] { "渦" }, IsAviUtlSupported = false)]
public sealed class VortexSwirlEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "渦巻き";

    Animation _progress = new Animation(42, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _turns = new Animation(2.2, 0.2, 6);
    [Display(Name = "回転量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 6)]
    public Animation Turns { get => _turns; set => Set(ref _turns, value); }

    Animation _falloff = new Animation(0.55, 0, 1);
    [Display(Name = "中心の残り", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Falloff { get => _falloff; set => Set(ref _falloff, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    bool _chroma = true;
    [Display(Name = "色収差", GroupName = "外観")]
    [ToggleSlider]
    public bool Chroma { get => _chroma; set => Set(ref _chroma, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<VortexSwirlCustomEffect>(devices, this, d => new VortexSwirlCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Turns, Falloff, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Turns.GetValue(f, len, fps),
            Size = (float)Falloff.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)CenterY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = Chroma ? 1f : 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
