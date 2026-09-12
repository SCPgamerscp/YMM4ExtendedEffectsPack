using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum TimeWarpScanAxis
{
    [Display(Name = "上から下")] Vertical = 0,
    [Display(Name = "左から右")] Horizontal = 1,
}

[VideoEffect("タイムワープスキャン", new[] { "拡張エフェクト", "演出" }, new[] { "スキャン" }, IsAviUtlSupported = false)]
public sealed class TimeWarpScanEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "タイムワープスキャン";

    Animation _progress = new Animation(42, 0, 100);
    [Display(Name = "スキャン位置", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _width = new Animation(0.04, 0.01, 0.2);
    [Display(Name = "ライン幅", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.01, 0.2)]
    public Animation Width { get => _width; set => Set(ref _width, value); }

    Animation _freeze = new Animation(1, 0, 1);
    [Display(Name = "フリーズ量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Freeze { get => _freeze; set => Set(ref _freeze, value); }

    Animation _warp = new Animation(0.16, 0, 0.5);
    [Display(Name = "歪み", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.5)]
    public Animation Warp { get => _warp; set => Set(ref _warp, value); }

    TimeWarpScanAxis _axis = TimeWarpScanAxis.Vertical;
    [Display(Name = "方向", GroupName = "基本")]
    [EnumComboBox]
    public TimeWarpScanAxis Axis
    {
        get => _axis;
        set => Set(ref _axis, value);
    }

    Color _tint = Color.FromRgb(94, 200, 255);
    [Display(Name = "スキャン色", GroupName = "外観")]
    [ColorPicker]
    public Color ScanColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<TimeWarpScanCustomEffect>(devices, this, d => new TimeWarpScanCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Width, Freeze, Warp];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Freeze.GetValue(f, len, fps),
            Size = (float)Width.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Warp.GetValue(f, len, fps),
            Mode = (float)(int)Axis,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = ScanColor.R / 255f,
            ColorG = ScanColor.G / 255f,
            ColorB = ScanColor.B / 255f,
        };
    }
}
