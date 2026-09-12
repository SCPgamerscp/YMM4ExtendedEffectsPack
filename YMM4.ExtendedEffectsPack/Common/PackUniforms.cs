using System.Runtime.InteropServices;

namespace YMM4.ExtendedEffectsPack.Common;

[StructLayout(LayoutKind.Sequential)]
public struct PackUniforms
{
    public float Strength;
    public float Size;
    public float Speed;
    public float Angle;
    public float Count;
    public float Mix;
    public float Spread;
    public float Mode;
    public float FlagA;
    public float FlagB;
    public float Time;
    public float Progress;
    public float ColorR;
    public float ColorG;
    public float ColorB;
    public float Pad;
}
