using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum PaperTearDirection
{
    [Display(Name = "左右に裂く")] Horizontal = 0,
    [Display(Name = "上下に裂く")] Vertical = 1,
}

[VideoEffect("紙破り", new[] { "拡張エフェクト", "演出" }, new[] { "紙" }, IsAviUtlSupported = false)]
public sealed class PaperTearEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "紙破り";

    Animation _progress = new Animation(40, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _jagged = new Animation(0.65, 0, 1);
    [Display(Name = "ギザギザ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Jagged { get => _jagged; set => Set(ref _jagged, value); }

    Animation _shadow = new Animation(0.5, 0, 1);
    [Display(Name = "裂け目の影", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Shadow { get => _shadow; set => Set(ref _shadow, value); }

    PaperTearDirection _direction = PaperTearDirection.Horizontal;
    [Display(Name = "方向", GroupName = "基本")]
    [EnumComboBox]
    public PaperTearDirection Direction
    {
        get => _direction;
        set => Set(ref _direction, value);
    }

    Color _tint = Color.FromRgb(230, 223, 210);
    [Display(Name = "裏面の色", GroupName = "外観")]
    [ColorPicker]
    public Color BackColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<PaperTearCustomEffect>(devices, this, d => new PaperTearCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Jagged, Shadow];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Shadow.GetValue(f, len, fps),
            Size = 0f,
            Speed = 0f,
            Angle = 0f,
            Count = 0f,
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)Jagged.GetValue(f, len, fps),
            Mode = (float)(int)Direction,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = BackColor.R / 255f,
            ColorG = BackColor.G / 255f,
            ColorB = BackColor.B / 255f,
        };
    }
}
