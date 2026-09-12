using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;

public enum TheaterCurtainOpening
{
    [Display(Name = "中央両開き")] Center = 0,
    [Display(Name = "左へ片引き")] Left = 1,
    [Display(Name = "右へ片引き")] Right = 2,
}

[VideoEffect("劇場カーテン", new[] { "拡張エフェクト", "演出" }, new[] { "カーテン", "開幕" }, IsAviUtlSupported = false)]
public sealed class TheaterCurtainEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "劇場カーテン";

    Animation _progress = new Animation(35, 0, 100);
    [Display(Name = "進行度", GroupName = "基本")]
    [AnimationSlider("F2", "%", 0, 100)]
    public Animation Progress { get => _progress; set => Set(ref _progress, value); }

    Animation _folds = new Animation(12, 4, 30);
    [Display(Name = "ドレープの数", GroupName = "基本")]
    [AnimationSlider("F2", "", 4, 30)]
    public Animation Folds { get => _folds; set => Set(ref _folds, value); }

    TheaterCurtainOpening _opening = TheaterCurtainOpening.Center;
    [Display(Name = "開き方", GroupName = "基本")]
    [EnumComboBox]
    public TheaterCurtainOpening Opening
    {
        get => _opening;
        set => Set(ref _opening, value);
    }

    bool _gloss = true;
    [Display(Name = "ベルベット光沢", GroupName = "外観")]
    [ToggleSlider]
    public bool Gloss { get => _gloss; set => Set(ref _gloss, value); }

    bool _fringe = true;
    [Display(Name = "金フリンジ", GroupName = "外観")]
    [ToggleSlider]
    public bool Fringe { get => _fringe; set => Set(ref _fringe, value); }

    Color _tint = Color.FromRgb(138, 11, 26);
    [Display(Name = "カーテン色", GroupName = "外観")]
    [ColorPicker]
    public Color Tint { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<TheaterCurtainCustomEffect>(devices, this, d => new TheaterCurtainCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [Progress, Folds];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = 0f,
            Size = 0f,
            Speed = 0f,
            Angle = 0f,
            Count = (float)Folds.GetValue(f, len, fps),
            Mix = (float)Progress.GetValue(f, len, fps),
            Spread = 0f,
            Mode = (float)(int)Opening,
            FlagA = Gloss ? 1f : 0f,
            FlagB = Fringe ? 1f : 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Progress.GetValue(f, len, fps),
            ColorR = Tint.R / 255f,
            ColorG = Tint.G / 255f,
            ColorB = Tint.B / 255f,
        };
    }
}
