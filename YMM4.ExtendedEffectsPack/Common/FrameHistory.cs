using System.Numerics;
using Vortice.Direct2D1;
using Vortice.Direct2D1.Effects;
using YukkuriMovieMaker.Commons;

namespace YMM4.ExtendedEffectsPack.Common;

/// <summary>
/// Bounded, processor-local temporal history from PR #1.
/// Only sequential playback is meaningful; seek/gap/size/delay changes clear the history.
/// </summary>
internal sealed class FrameHistory : IDisposable
{
    private const long BudgetBytes = 128L * 1024 * 1024;
    private readonly IGraphicsDevicesAndContext devices;
    private readonly AffineTransform2D restore;
    private readonly ID2D1Image output;
    private ID2D1Bitmap?[] frames = [];
    private int[] frameNumbers = [];
    private int previous = -1, delay;
    private Vector4 rect;
    private float scale = 1;
    private bool disposed;

    public FrameHistory(IGraphicsDevicesAndContext devices)
    {
        this.devices = devices;
        restore = new AffineTransform2D(devices.DeviceContext);
        output = restore.Output;
    }

    public ID2D1Image? Update(ID2D1Image input, Vector4 bounds, int frame, int requestedDelay)
    {
        if (frame < 0) { Reset(); return null; }
        if (bounds != rect || requestedDelay != delay || (previous >= 0 && frame != previous && frame != previous + 1))
        {
            Reset();
            rect = bounds;
            delay = requestedDelay;
            frames = new ID2D1Bitmap?[delay + 1];
            frameNumbers = Enumerable.Repeat(-1, delay + 1).ToArray();
            scale = (float)Math.Min(1, Math.Sqrt(BudgetBytes / ((delay + 1.0) * Math.Max(1, bounds.Z) * Math.Max(1, bounds.W) * 8)));
            restore.TransformMatrix = Matrix3x2.CreateScale(1 / scale) * Matrix3x2.CreateTranslation(bounds.X, bounds.Y);
        }
        restore.SetInput(0, null, true);
        if (frame != previous)
        {
            int slot = frame % frames.Length;
            frames[slot] ??= devices.DeviceContext.CreateEmptyBitmap(
                Math.Max(1, (int)Math.Floor(rect.Z * scale)), Math.Max(1, (int)Math.Floor(rect.W * scale)),
                format: Vortice.DXGI.Format.R16G16B16A16_Float, pixelBytes: 8);
            Capture(input, frames[slot]!);
            frameNumbers[slot] = frame;
            previous = frame;
        }
        int pastFrame = frame - delay;
        if (pastFrame < 0) return null;
        int pastSlot = pastFrame % frames.Length;
        if (frameNumbers[pastSlot] != pastFrame) return null;
        restore.SetInput(0, frames[pastSlot], true);
        return output;
    }

    private void Capture(ID2D1Image input, ID2D1Bitmap bitmap)
    {
        var dc = devices.DeviceContext;
        using var oldTarget = dc.Target;
        var oldTransform = dc.Transform;
        bool drawing = false;
        try
        {
            dc.Target = bitmap;
            dc.Transform = Matrix3x2.CreateTranslation(-rect.X, -rect.Y) * Matrix3x2.CreateScale(scale);
            dc.BeginDraw(); drawing = true;
            dc.Clear(null);
            dc.DrawImage(input);
            drawing = false;
            dc.EndDraw();
        }
        finally
        {
            try { if (drawing) dc.EndDraw(); }
            finally { dc.Transform = oldTransform; dc.Target = oldTarget; }
        }
    }

    public void Clear() => Reset();

    private void Reset()
    {
        restore.SetInput(0, null, true);
        foreach (var bitmap in frames) bitmap?.Dispose();
        frames = [];
        frameNumbers = [];
        previous = -1;
        rect = default;
        delay = 0;
    }

    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        Reset();
        output.Dispose();
        restore.Dispose();
    }
}
