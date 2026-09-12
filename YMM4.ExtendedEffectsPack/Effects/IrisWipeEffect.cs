using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum IrisWipeShape
{
    [Display(Name = "円")] Circle = 0,
    [Display(Name = "多角形")] Poly = 1,
}

[VideoEffect("アイリスワイプ", new[] { "拡張エフェクト", "演出" }, new[] { "ワイプ" }, IsAviUtlSupported = false)]
public sealed class IrisWipeEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "アイリスワイプ";

    Animation _progress = new Animation(45, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _sides = new Animation(6, 3, 12);
    [Display(Name = "辺の数", GroupName = "基本")]
    [AnimationSlider("F2", "", 3, 12)]
    public Animation Sides { get => _sides; set => Set(ref _sides, value); }

    Animation _softness = new Animation(0.06, 0, 0.2);
    [Display(Name = "縁のぼかし", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 0.2)]
    public Animation Softness { get => _softness; set => Set(ref _softness, value); }

    Animation _centerX = new Animation(0.5, 0, 1);
    [Display(Name = "中心 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterX { get => _centerX; set => Set(ref _centerX, value); }

    Animation _centerY = new Animation(0.5, 0, 1);
    [Display(Name = "中心 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation CenterY { get => _centerY; set => Set(ref _centerY, value); }

    IrisWipeShape _shape = IrisWipeShape.Circle;
    [Display(Name = "形状", GroupName = "基本")]
    [EnumComboBox]
    public IrisWipeShape Shape
    {
        get => _shape;
        set => Set(ref _shape, value);
    }

    Color _tint = Color.FromRgb(5, 5, 6);
    [Display(Name = "マスク色", GroupName = "外観")]
    [ColorPicker]
    public Color MaskColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<IrisWipeCustomEffect>(devices, this, d => new IrisWipeCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Sides, Softness, CenterX, CenterY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)Softness.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)CenterX.GetValue(f, len, fps),
            Count = (float)Sides.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)CenterY.GetValue(f, len, fps),
            Mode = (float)(int)Shape,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = MaskColor.R / 255f,
            ColorG = MaskColor.G / 255f,
            ColorB = MaskColor.B / 255f,
        };
    }
}
