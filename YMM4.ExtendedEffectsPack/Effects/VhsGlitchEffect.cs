using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("レトロVHS", new[] { "拡張エフェクト", "加工" }, new[] { "VHS" }, IsAviUtlSupported = false)]
public sealed class VhsGlitchEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "レトロVHS";

    Animation _intensity = new Animation(0.3, 0, 1);
    [Display(Name = "グリッチ強度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Intensity { get => _intensity; set => Set(ref _intensity, value); }

    Animation _scan = new Animation(0.4, 0, 1);
    [Display(Name = "走査線濃度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Scan { get => _scan; set => Set(ref _scan, value); }

    Animation _sync = new Animation(0.35, 0, 1);
    [Display(Name = "水平同期ズレ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Sync { get => _sync; set => Set(ref _sync, value); }

    bool _tracking = true;
    [Display(Name = "下部トラッキング", GroupName = "外観")]
    [ToggleSlider]
    public bool Tracking { get => _tracking; set => Set(ref _tracking, value); }

    bool _tapeFade = true;
    [Display(Name = "テープ褪色", GroupName = "外観")]
    [ToggleSlider]
    public bool TapeFade { get => _tapeFade; set => Set(ref _tapeFade, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<VhsGlitchCustomEffect>(devices, this, d => new VhsGlitchCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Intensity, Scan, Sync];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Intensity.GetValue(f, len, fps),
            Size = 0f,
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Scan.GetValue(f, len, fps),
            Spread = (float)Sync.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = Tracking ? 1f : 0f,
            FlagB = TapeFade ? 1f : 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Scan.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
