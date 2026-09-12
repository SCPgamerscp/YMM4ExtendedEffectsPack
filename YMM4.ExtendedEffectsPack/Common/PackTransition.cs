using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Controls;
using YukkuriMovieMaker.Player.Video;
using YukkuriMovieMaker.Plugin.Transition;
using System.ComponentModel.DataAnnotations;

namespace YMM4.ExtendedEffectsPack.Common;

public enum TransitionEnvelope
{
    /// <summary>Mix is "open amount": 100 at both ends, 0 at the midpoint (curtain / blink / iris).</summary>
    CloseOpen,
    /// <summary>Mix peaks at the midpoint so the cut is covered (ink / paper / melt).</summary>
    PeakCover,
    /// <summary>Mix is linear 0→100; the shader already peaks at 50% (whip / glitch / zoom).</summary>
    ShaderPeak,
}

public abstract class PackTransitionParameter : TransitionParameterBase
{
    EasingType _easingType = EasingType.Cubic;
    [Display(Name = "緩急", GroupName = "切替", Order = 90)]
    [EnumComboBox]
    public EasingType EasingType { get => _easingType; set => Set(ref _easingType, value); }

    EasingMode _easingMode = EasingMode.InOut;
    [Display(Name = "加減速", GroupName = "切替", Order = 91)]
    [EnumComboBox]
    public EasingMode EasingMode { get => _easingMode; set => Set(ref _easingMode, value); }

    public abstract TransitionEnvelope Envelope { get; }
    public abstract PackUniforms BuildStyle(double frame, double length, int fps);

    public PackUniforms BuildUniforms(double frame, double length, int fps)
    {
        double raw = length <= 1 ? 1 : Math.Clamp(frame / (length - 1), 0, 1);
        float p = (float)Math.Clamp(Easing.GetValue(EasingType, EasingMode, raw), 0, 1);
        if (raw <= 0) p = 0;
        if (raw >= 1) p = 1;
        var u = BuildStyle(frame, length, fps);
        u.Time = (float)(frame / Math.Max(1, fps));
        u.Pad = p;
        u.Mix = Envelope switch
        {
            TransitionEnvelope.CloseOpen => MathF.Abs(2 * p - 1) * 100f,
            TransitionEnvelope.PeakCover => (1 - MathF.Abs(2 * p - 1)) * 100f,
            _ => p * 100f,
        };
        u.Progress = u.Mix;
        return u;
    }
}

internal sealed class PackTransitionSource<TEffect> : ITransitionSource
    where TEffect : D2D1CustomShaderEffectBase, IPackShader
{
    readonly ID2D1Image before, after;
    readonly PackTransitionParameter item;
    readonly TEffect? effect;
    readonly ID2D1Image? output;
    bool disposed;
    public ID2D1Image Output => output ?? before;

    public PackTransitionSource(
        IGraphicsDevicesAndContext devices,
        ID2D1Image before,
        ID2D1Image after,
        PackTransitionParameter item,
        Func<IGraphicsDevicesAndContext, TEffect> factory)
    {
        this.before = before;
        this.after = after;
        this.item = item;
        var e = factory(devices);
        if (!e.IsEnabled)
        {
            e.Dispose();
            effect = null;
            output = null;
        }
        else
        {
            effect = e;
            output = e.Output;
        }
    }

    public void Update(TimelineItemSourceDescription desc)
    {
        if (effect is null) return;
        var u = item.BuildUniforms(desc.ItemPosition.Frame, desc.ItemDuration.Frame, desc.FPS);
        var source = u.Pad < 0.5f ? before : after;
        effect.SetInput(0, source, true);
        effect.SetUniforms(u);
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        effect?.SetInput(0, null, true);
        effect?.Dispose();
        output?.Dispose();
    }
}
