using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("カレイドスコープ", new[] { "拡張エフェクト", "加工" }, new[] { "万華鏡" }, IsAviUtlSupported = false)]
public sealed class KaleidoscopeEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "カレイドスコープ";

    Animation _slices = new Animation(6, 2, 16);
    [Display(Name = "分割数", GroupName = "基本")]
    [AnimationSlider("F2", "", 2, 16)]
    public Animation Slices { get => _slices; set => Set(ref _slices, value); }

    Animation _zoom = new Animation(1.05, 0.6, 2);
    [Display(Name = "ズーム", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.6, 2)]
    public Animation Zoom { get => _zoom; set => Set(ref _zoom, value); }

    Animation _spin = new Animation(0.15, -2, 2);
    [Display(Name = "回転速度", GroupName = "基本")]
    [AnimationSlider("F2", "", -2, 2)]
    public Animation Spin { get => _spin; set => Set(ref _spin, value); }

    Animation _offsetX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OffsetX { get => _offsetX; set => Set(ref _offsetX, value); }

    Animation _offsetY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OffsetY { get => _offsetY; set => Set(ref _offsetY, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<KaleidoscopeCustomEffect>(devices, this, d => new KaleidoscopeCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Slices, Zoom, Spin, OffsetX, OffsetY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)Zoom.GetValue(f, len, fps),
            Speed = (float)Spin.GetValue(f, len, fps),
            Angle = (float)OffsetX.GetValue(f, len, fps),
            Count = (float)Slices.GetValue(f, len, fps),
            Mix = 0f,
            Spread = (float)OffsetY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
