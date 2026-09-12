using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum PixelateShape
{
    [Display(Name = "四角形")] Square = 0,
    [Display(Name = "円形ドット")] Circle = 1,
    [Display(Name = "六角形")] Hex = 2,
}

public enum PixelateReduce
{
    [Display(Name = "OFF")] Off = 0,
    [Display(Name = "16色")] C16 = 1,
    [Display(Name = "64色")] C64 = 2,
    [Display(Name = "ゲームボーイ4階調")] Gb = 3,
}

[VideoEffect("ピクセル化", new[] { "拡張エフェクト", "加工" }, new[] { "モザイク", "レトロ" }, IsAviUtlSupported = false)]
public sealed class PixelateEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ピクセル化";

    Animation _pixelSize = new Animation(16, 2, 80);
    [Display(Name = "ピクセルサイズ", GroupName = "基本")]
    [AnimationSlider("F2", "px", 2, 80)]
    public Animation PixelSize { get => _pixelSize; set => Set(ref _pixelSize, value); }

    Animation _gridOpacity = new Animation(0.35, 0, 1);
    [Display(Name = "グリッド不透明度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation GridOpacity { get => _gridOpacity; set => Set(ref _gridOpacity, value); }

    PixelateShape _shape = PixelateShape.Square;
    [Display(Name = "ピクセル形状", GroupName = "基本")]
    [EnumComboBox]
    public PixelateShape Shape
    {
        get => _shape;
        set => Set(ref _shape, value);
    }

    PixelateReduce _reduce = PixelateReduce.Off;
    [Display(Name = "レトロ減色", GroupName = "基本")]
    [EnumComboBox]
    public PixelateReduce Reduce
    {
        get => _reduce;
        set => Set(ref _reduce, value);
    }

    bool _grid = false;
    [Display(Name = "グリッド線", GroupName = "外観")]
    [ToggleSlider]
    public bool Grid { get => _grid; set => Set(ref _grid, value); }

    bool _dither = false;
    [Display(Name = "ディザリング", GroupName = "外観")]
    [ToggleSlider]
    public bool Dither { get => _dither; set => Set(ref _dither, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<PixelateCustomEffect>(devices, this, d => new PixelateCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [PixelSize, GridOpacity];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = (float)PixelSize.GetValue(f, len, fps),
            Speed = 0f,
            Angle = 0f,
            Count = (float)(int)Reduce,
            Mix = (float)GridOpacity.GetValue(f, len, fps),
            Spread = 0f,
            Mode = (float)(int)Shape,
            FlagA = Grid ? 1f : 0f,
            FlagB = Dither ? 1f : 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)GridOpacity.GetValue(f, len, fps),
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
