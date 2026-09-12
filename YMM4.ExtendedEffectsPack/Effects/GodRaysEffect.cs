using System.ComponentModel.DataAnnotations;
using System.Windows.Media;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Exo;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Effects;
using YMM4.ExtendedEffectsPack.Common;

namespace YMM4.ExtendedEffectsPack.Effects;


[VideoEffect("ゴッドレイ", new[] { "拡張エフェクト", "描画" }, new[] { "光線" }, IsAviUtlSupported = false)]
public sealed class GodRaysEffect : VideoEffectBase, IPackBindable
{
    public override string Label => "ゴッドレイ";

    Animation _lightX = new Animation(0.5, -0.2, 1.2);
    [Display(Name = "光源 X", GroupName = "基本")]
    [AnimationSlider("F2", "", -0.2, 1.2)]
    public Animation LightX { get => _lightX; set => Set(ref _lightX, value); }

    Animation _lightY = new Animation(-0.05, -0.3, 1.2);
    [Display(Name = "光源 Y", GroupName = "基本")]
    [AnimationSlider("F2", "", -0.3, 1.2)]
    public Animation LightY { get => _lightY; set => Set(ref _lightY, value); }

    Animation _density = new Animation(24, 5, 50);
    [Display(Name = "光筋の密度", GroupName = "基本")]
    [AnimationSlider("F2", "", 5, 50)]
    public Animation Density { get => _density; set => Set(ref _density, value); }

    Animation _rayLength = new Animation(0.85, 0.1, 1);
    [Display(Name = "光条の長さ", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.1, 1)]
    public Animation RayLength { get => _rayLength; set => Set(ref _rayLength, value); }

    Animation _slit = new Animation(0.6, 0, 1);
    [Display(Name = "雲間スリット", GroupName = "基本")]
    [AnimationSlider("F2", "", 0, 1)]
    public Animation Slit { get => _slit; set => Set(ref _slit, value); }

    Animation _exposure = new Animation(1.1, 0.2, 2.5);
    [Display(Name = "露出", GroupName = "基本")]
    [AnimationSlider("F2", "", 0.2, 2.5)]
    public Animation Exposure { get => _exposure; set => Set(ref _exposure, value); }

    Color _tint = Color.FromRgb(255, 243, 208);
    [Display(Name = "光の色", GroupName = "外観")]
    [ColorPicker]
    public Color RayColor { get => _tint; set => Set(ref _tint, value); }

    public override IEnumerable<string> CreateExoVideoFilters(int keyFrameIndex, ExoOutputDescription exoOutputDescription) => [];
    public override IVideoEffectProcessor CreateVideoEffect(IGraphicsDevicesAndContext devices)
        => new PackProcessor<GodRaysCustomEffect>(devices, this, d => new GodRaysCustomEffect(d));
    protected override IEnumerable<IAnimatable> GetAnimatables() => [LightX, LightY, Density, RayLength, Slit, Exposure];

    public PackUniforms BuildUniforms(EffectDescription d)
    {
        var f = d.ItemPosition.Frame; var len = d.ItemDuration.Frame; var fps = d.FPS;
        return new PackUniforms {
            Strength = (float)Exposure.GetValue(f, len, fps),
            Size = (float)RayLength.GetValue(f, len, fps),
            Speed = 0f,
            Angle = (float)LightX.GetValue(f, len, fps),
            Count = (float)Density.GetValue(f, len, fps),
            Mix = (float)Slit.GetValue(f, len, fps),
            Spread = (float)LightY.GetValue(f, len, fps),
            Mode = 0f,
            FlagA = 0f,
            FlagB = 0f,
            Time = len > 0 && fps > 0 ? (float)f / fps : 0f,
            Progress = (float)Slit.GetValue(f, len, fps),
            ColorR = RayColor.R / 255f,
            ColorG = RayColor.G / 255f,
            ColorB = RayColor.B / 255f,
        };
    }
}
