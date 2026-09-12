using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum HeartbeatBeatMode
{
    [Display(Name = "リアル心電（ドッ・クン）")] Ecg = 0,
    [Display(Name = "シンプル弾み")] Sine = 1,
}

[VideoEffect("鼓動", new[] { "拡張エフェクト", "演出" }, new[] { "心拍" }, IsAviUtlSupported = false)]
public sealed class HeartbeatEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "鼓動";

    Animation _bpm = new Animation(75, 30, 240);
    [Display(Name = "BPM", GroupName = "基本")]
    [AnimationSlider("F2", "", 30, 240)]
    public Animation Bpm { get => _bpm; set => Set(ref _bpm, value); }

    Animation _scale = new Animation(8, 0, 30);
    [Display(Name = "拡縮強度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 30)]
    public Animation Scale { get => _scale; set => Set(ref _scale, value); }

    Animation _flash = new Animation(0.5, 0, 1);
    [Display(Name = "周辺フラッシュ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Flash { get => _flash; set => Set(ref _flash, value); }

    Animation _chroma = new Animation(5, 0, 15);
    [Display(Name = "色収差ブレ", GroupName = "基本")]
    [AnimationSlider("F2", "px", 0, 15)]
    public Animation Chroma { get => _chroma; set => Set(ref _chroma, value); }

    HeartbeatBeatMode _beatMode = HeartbeatBeatMode.Ecg;
    [Display(Name = "鼓動モード", GroupName = "基本")]
    [EnumComboBox]
    public HeartbeatBeatMode BeatMode
    {
        get => _beatMode;
        set => Set(ref _beatMode, value);
    }

    Color _tint = Color.FromRgb(128, 0, 0);
    [Display(Name = "フラッシュ色", GroupName = "外観")]
    [ColorPicker]
    public Color FlashColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<HeartbeatCustomEffect>(devices, this, d => new HeartbeatCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Bpm, Scale, Flash, Chroma];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Scale.GetValue(f, len, fps),
            Size = 0f,
            Speed = (float)Bpm.GetValue(f, len, fps),
            Angle = 0f,
            Count = 0f,
            Mix = (float)Flash.GetValue(f, len, fps),
            Spread = (float)Chroma.GetValue(f, len, fps),
            Mode = (float)(int)BeatMode,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Flash.GetValue(f, len, fps),
            ColorR = FlashColor.R / 255f,
            ColorG = FlashColor.G / 255f,
            ColorB = FlashColor.B / 255f,
        };
    }
}
