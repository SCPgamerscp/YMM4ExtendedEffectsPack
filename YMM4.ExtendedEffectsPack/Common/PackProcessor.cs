using Vortice.Direct2D1;
using YukkuriMovieMaker.Commons;
using YukkuriMovieMaker.Player.Video;

namespace YMM4.ExtendedEffectsPack.Common;

internal sealed class PackProcessor<TEffect> : IVideoEffectProcessor where TEffect : D2D1CustomShaderEffectBase, IPackShader
{
    readonly IPackBindable item;
    readonly TEffect? effect;
    readonly ID2D1Image? output;
    ID2D1Image? input;

    public ID2D1Image Output => output ?? input ?? throw new NullReferenceException();

    public PackProcessor(IGraphicsDevicesAndContext devices, IPackBindable item, Func<IGraphicsDevicesAndContext, TEffect> factory)
    {
        this.item = item;
        var e = factory(devices);
        if (!e.IsEnabled)
        {
            e.Dispose();
            effect = null;
        }
        else
        {
            effect = e;
            output = e.Output;
        }
    }

    public void SetInput(ID2D1Image? input)
    {
        this.input = input;
        effect?.SetInput(0, input, true);
    }

    public void ClearInput() => effect?.SetInput(0, null, true);

    public DrawDescription Update(EffectDescription effectDescription)
    {
        if (effect is null) return effectDescription.DrawDescription;
        effect.SetUniforms(item.BuildUniforms(effectDescription));
        return effectDescription.DrawDescription;
    }

    public void Dispose()
    {
        output?.Dispose();
        effect?.SetInput(0, null, true);
        effect?.Dispose();
    }
}

internal interface IPackBindable
{
    PackUniforms BuildUniforms(EffectDescription desc);
}

internal interface IPackShader
{
    void SetUniforms(PackUniforms u);
}
