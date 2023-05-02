using System.Runtime.InteropServices;

namespace TinCan.NET;

[StructLayout(LayoutKind.Explicit)]
public struct Buttons
{
    [Flags]
    public enum Mask
    {
        A = 0x8000,
        B = 0x4000,
        Z = 0x2000,
        Start = 0x1000,
        DUp = 0x0800,
        DDown = 0x0400,
        DLeft = 0x0200,
        DRight = 0x0100,
        Unknown1 = 0x0080,
        Unknown2 = 0x0040,
        L = 0x0020,
        R = 0x0010,
        CUp = 0x0008,
        CDown = 0x0004,
        CLeft = 0x0002,
        CRight = 0x0001,
    }

    [FieldOffset(0)]
    public Mask BtnMask;
    
    [FieldOffset(2)]
    public sbyte JoyX;
    
    [FieldOffset(3)]
    public sbyte JoyY;
    
    [FieldOffset(0)]
    public int Value;
}

[StructLayout(LayoutKind.Sequential)]
public struct Control
{
    public enum PakType
    {
        None = 1,
        Mempak = 2,
        RumblePak = 3,
        TransferPak = 4,
        Raw = 5,
        BioPak = 6
    }

    public enum ControllerType
    {
        Standard = 0,
        VRU = 1
    }
    
    public int Present;
    public int RawData;
    public PakType Plugin;
    public ControllerType Type;
    
    
}

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ControlInfo
{
    public Control* Controls;
}