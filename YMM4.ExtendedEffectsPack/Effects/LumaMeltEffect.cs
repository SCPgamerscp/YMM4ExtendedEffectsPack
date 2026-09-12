using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum LumaMeltDirection
{
    [Display(Name = "下へ垂れる")] Down = 0,
    [Display(Name = "放射に溶ける")] Radial = 1,
}

[VideoEffect("ルマメルト", new[] { "拡張エフェクト", "演出" }, new[] { "溶解" }, IsAviUtlSupported = false)]
public sealed class LumaMeltEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ルマメルト";

    Animation _progress = new Animation(50, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _amount = new Animation(0.7, 0, 1);
    [Display(Name = "溶ける量", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Amount { get => _amount; set => Set(ref _amount, value); }

    Animation _turbulence = new Animation(0.55, 0, 1);
    [Display(Name = "液の乱れ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Turbulence { get => _turbulence; set => Set(ref _turbulence, value); }

    LumaMeltDirection _direction = LumaMeltDirection.Down;
    [Display(Name = "方向", GroupName = "基本")]
    [EnumComboBox]
    public LumaMeltDirection Direction
    {
        get => _direction;
        set => Set(ref _direction, value);
    }

    Color _tint = Color.FromRgb(26, 14, 8);
    [Display(Name = "溶け跡の色", GroupName = "外観")]
    [ColorPicker]
    public Color MeltColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<LumaMeltCustomEffect>(devices, this, d => new LumaMeltCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Amount, Turbulence];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amount.GetValue(f, len, fps),
            Size = 0f,
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Turbulence.GetValue(f, len, fps),
            Mode = (float)(int)Direction,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = MeltColor.R / 255f,
            ColorG = MeltColor.G / 255f,
            ColorB = MeltColor.B / 255f,
        };
    }
}
