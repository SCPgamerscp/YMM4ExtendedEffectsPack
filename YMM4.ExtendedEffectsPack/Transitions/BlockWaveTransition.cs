using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Transition;
using YMM4.ExtendedEffectsPack.Common;
using YMM4.ExtendedEffectsPack.Effects;

namespace YMM4.ExtendedEffectsPack.Transitions;

public sealed class BlockWaveTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / ブロックウェーブ";
    public ITransitionParameter CreateTransitionParameter() => new BlockWaveTransitionParameter();
}

public sealed class BlockWaveTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.ShaderPeak;

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
    public BlockWaveDirection Direction { get => _direction; set => Set(ref _direction, value); }

    BlockWaveWaveShape _waveShape = BlockWaveWaveShape.Sine;
    [Display(Name = "波の形状", GroupName = "基本")]
    [EnumComboBox]
    public BlockWaveWaveShape WaveShape { get => _waveShape; set => Set(ref _waveShape, value); }

    bool _border = true;
    [Display(Name = "ブロック境界線", GroupName = "外観")]
    [ToggleSlider]
    public bool Border { get => _border; set => Set(ref _border, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<BlockWaveCustomEffect>(devices, before, after, this, d => new BlockWaveCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Blocks, Amplitude, WaveSpeed];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = (float)Amplitude.GetValue(frame, length, fps), Size = 0f,
            Speed = (float)WaveSpeed.GetValue(frame, length, fps), Angle = 0f,
            Count = (float)Blocks.GetValue(frame, length, fps),
            Spread = (float)(int)WaveShape, Mode = (float)(int)Direction,
            FlagA = Border ? 1f : 0f, FlagB = 0f,
            ColorR = 232/255f, ColorG = 230/255f, ColorB = 225/255f,
    };
}
