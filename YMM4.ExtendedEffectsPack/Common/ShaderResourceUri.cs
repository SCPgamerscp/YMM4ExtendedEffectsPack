using System.Reflection;

namespace YMM4.ExtendedEffectsPack.Common;

internal static class ShaderResourceUri
{
    public static byte[] Get(string shaderName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var suffix = $".{shaderName}.cso";
        var resource = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Embedded shader not found: {shaderName}.cso");
        using var stream = assembly.GetManifestResourceStream(resource)
            ?? throw new InvalidOperationException($"Cannot open {resource}");
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        return ms.ToArray();
    }
}
