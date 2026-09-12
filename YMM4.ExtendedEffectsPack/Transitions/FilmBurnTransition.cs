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

public sealed class FilmBurnTransitionPlugin : ITransitionPlugin
{
    public string Name => "EEP / フィルムバーン";
    public ITransitionParameter CreateTransitionParameter() => new FilmBurnTransitionParameter();
}

public sealed class FilmBurnTransitionParameter : PackTransitionParameter
{
    public override TransitionEnvelope Envelope => TransitionEnvelope.PeakCover;

    Animation _width = new Animation(0.8, 0.1, 2);
    [Display(Name = "感光の広がり", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 2)]
    public Animation Width { get => _width; set => Set(ref _width, value); }

    FilmBurnTone _tone = FilmBurnTone.Warm;
    [Display(Name = "光の色調", GroupName = "基本")]
    [EnumComboBox]
    public FilmBurnTone Tone { get => _tone; set => Set(ref _tone, value); }

    bool _grain = true;
    [Display(Name = "フィルム粒子", GroupName = "外観")]
    [ToggleSlider]
    public bool Grain { get => _grain; set => Set(ref _grain, value); }

    bool _burn = true;
    [Display(Name = "焼きつき暗転", GroupName = "外観")]
    [ToggleSlider]
    public bool Burn { get => _burn; set => Set(ref _burn, value); }

    public override ITransitionSource CreateTransition(IGraphicsDevicesAndContext devices, ID2D1Image before, ID2D1Image after)
        => new PackTransitionSource<FilmBurnCustomEffect>(devices, before, after, this, d => new FilmBurnCustomEffect(d));

    protected override IEnumerable<IAnimatable> GetAnimatables() => [Width];

    public override PackUniforms BuildStyle(double frame, double length, int fps) => new PackUniforms
    {

            Strength = 0f, Size = (float)Width.GetValue(frame, length, fps), Speed = 0f, Angle = 0f,
            Count = 0f, Spread = 0f, Mode = (float)(int)Tone,
            FlagA = Grain ? 1f : 0f, FlagB = Burn ? 1f : 0f,
            ColorR = 1f, ColorG = 160/255f, ColorB = 60/255f,
    };
}
