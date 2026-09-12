using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("インクスプラッシュ", new[] { "拡張エフェクト", "演出" }, new[] { "インク" }, IsAviUtlSupported = false)]
public sealed class InkBleedEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "インクスプラッシュ";

    Animation _progress = new Animation(48, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _spreadAmt = new Animation(0.55, 0, 1);
    [Display(Name = "にじみ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation SpreadAmt { get => _spreadAmt; set => Set(ref _spreadAmt, value); }

    Animation _veins = new Animation(0.65, 0, 1);
    [Display(Name = "葉脈の複雑さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Veins { get => _veins; set => Set(ref _veins, value); }

    Animation _originX = new Animation(0.5, 0, 1);
    [Display(Name = "発生点 X", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OriginX { get => _originX; set => Set(ref _originX, value); }

    Animation _originY = new Animation(0.5, 0, 1);
    [Display(Name = "発生点 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation OriginY { get => _originY; set => Set(ref _originY, value); }

    Color _tint = Color.FromRgb(17, 17, 20);
    [Display(Name = "インク色", GroupName = "外観")]
    [ColorPicker]
    public Color InkColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<InkBleedCustomEffect>(devices, this, d => new InkBleedCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, SpreadAmt, Veins, OriginX, OriginY];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)OriginY.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)OriginX.GetValue(f, len, fps),
            Count = (float)Veins.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = (float)SpreadAmt.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = InkColor.R / 255f,
            ColorG = InkColor.G / 255f,
            ColorB = InkColor.B / 255f,
        };
    }
}
