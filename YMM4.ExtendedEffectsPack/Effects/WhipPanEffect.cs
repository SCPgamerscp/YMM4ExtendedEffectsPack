using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("ホイップパン", new[] { "拡張エフェクト", "演出" }, new[] { "ホイップ" }, IsAviUtlSupported = false)]
public sealed class WhipPanEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ホイップパン";

    Animation _progress = new Animation(42, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _direction = new Animation(0, 0, 180);
    [Display(Name = "方向", GroupName = "基本")]
    [AnimationSlider("F2", "°", 0, 180)]
    public Animation Direction { get => _direction; set => Set(ref _direction, value); }

    Animation _smear = new Animation(0.85, 0, 1.5);
    [Display(Name = "ブラー量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1.5)]
    public Animation Smear { get => _smear; set => Set(ref _smear, value); }

    Animation _chroma = new Animation(0.4, 0, 1);
    [Display(Name = "色収差", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    bool _blackout = true;
    [Display(Name = "中間ブラックアウト", GroupName = "外観")]
    [ToggleSlider]
    public bool Blackout { get => _blackout; set => Set(ref _blackout, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<WhipPanCustomEffect>(devices, this, d => new WhipPanCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Direction, Smear, Chroma];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Smear.GetValue(f, len, fps),
            Size = 0f,
            Speed = 0f,
            Angle = (float)Direction.GetValue(f, len, fps),
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Chroma.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = Blackout ? 1f : 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
