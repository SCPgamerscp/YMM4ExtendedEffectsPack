using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum BlockWaveDirection
{
    [Display(Name = "水平ウェーブ")] Horizontal = 0,
    [Display(Name = "垂直ウェーブ")] Vertical = 1,
    [Display(Name = "中央放射波紋")] Radial = 2,
}

public enum BlockWaveWaveShape
{
    [Display(Name = "滑らかな正弦波")] Sine = 0,
    [Display(Name = "カクカク（デジタル）")] Step = 1,
}

[VideoEffect("ブロックウェーブ", new[] { "拡張エフェクト", "加工" }, new[] { "ウェーブ" }, IsAviUtlSupported = false)]
public sealed class BlockWaveEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ブロックウェーブ";

    Animation _blocks = new Animation(20, 5, 80);
    [Display(Name = "分割数", GroupName = "基本")]
    [AnimationSlider("F2", "", 5, 80)]
    public Animation Blocks { get => _blocks; set => Set(ref _blocks, value); }

    Animation _amplitude = new Animation(30, 0, 150);
    [Display(Name = "ズレ強度", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 150)]
    public Animation Amplitude { get => _amplitude; set => Set(ref _amplitude, value); }

    Animation _waveSpeed = new Animation(2, 0.1, 10);
    [Display(Name = "波の速度", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 10)]
    public Animation WaveSpeed { get => _waveSpeed; set => Set(ref _waveSpeed, value); }

    BlockWaveDirection _direction = BlockWaveDirection.Horizontal;
    [Display(Name = "進行方向", GroupName = "基本")]
    [EnumComboBox]
    public BlockWaveDirection Direction
    {
        get => _direction;
        set => Set(ref _direction, value);
    }

    BlockWaveWaveShape _waveShape = BlockWaveWaveShape.Sine;
    [Display(Name = "波の形状", GroupName = "基本")]
    [EnumComboBox]
    public BlockWaveWaveShape WaveShape
    {
        get => _waveShape;
        set => Set(ref _waveShape, value);
    }

    bool _border = true;
    [Display(Name = "ブロック境界線", GroupName = "外観")]
    [ToggleSlider]
    public bool Border { get => _border; set => Set(ref _border, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<BlockWaveCustomEffect>(devices, this, d => new BlockWaveCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Blocks, Amplitude, WaveSpeed];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Amplitude.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)WaveSpeed.GetValue(f, len, fps),
            Angle = 0f,
            Count = (float)Blocks.GetValue(f, len, fps),
            Mix = 0f,
            Spread = (float)(int)WaveShape,
            Mode = (float)(int)Direction,
            FlagA = Border ? 1f : 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = 0f,
            ColorR = 232/255f,
            ColorG = 230/255f,
            ColorB = 225/255f,
        };
    }
}
