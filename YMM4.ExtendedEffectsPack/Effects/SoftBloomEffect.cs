using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("ソフトブルーム", new[] { "拡張エフェクト", "描画" }, new[] { "ブルーム" }, IsAviUtlSupported = false)]
public sealed class SoftBloomEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ソフトブルーム";

    Animation _amount = new Animation(0.65, 0, 2);
    [Display(Name = "ブルーム量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 2)]
    public Animation Amount { get => _amount; set => Set(ref _amount, value); }

    Animation _radius = new Animation(16, 4, 40);
    [Display(Name = "にじみ半径", GroupName = "基本")]
    [AnimationSlider("F2", "", 4, 40)]
    public Animation Radius { get => _radius; set => Set(ref _radius, value); }

    Animation _threshold = new Animation(0.45, 0.1, 0.9);
    [Display(Name = "しきい値", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 0.9)]
    public Animation Threshold { get => _threshold; set => Set(ref _threshold, value); }

    Animation _warmth = new Animation(0.12, -0.4, 0.6);
    [Display(Name = "温かさ", GroupName = "基本")]
    [AnimationSlider("F2", "", -0.4, 0.6)]
    public Animation Warmth { get => _warmth; set => Set(ref _warmth, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<SoftBloomCustomEffect>(devices, this, d => new SoftBloomCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Amount, Radius, Threshold, Warmth];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amount.GetValue(f, len, fps),
            Size = (float)Radius.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Threshold.GetValue(f, len, fps),
            Spread = (float)Warmth.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Threshold.GetValue(f, len, fps),
            ColorR = 255/255f,
            ColorG = 244/255f,
            ColorB = 220/255f,
        };
    }
}
