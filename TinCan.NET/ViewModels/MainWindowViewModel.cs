using System;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TinCan.NET.Helpers;
using TinCan.NET.Models;

namespace TinCan.NET.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private sbyte _joyX;
    [ObservableProperty] private sbyte _joyY;

    private Buttons.Mask _buttonMask;

    // a mask of buttons which currently have rapidfire enabled 
    private Buttons.Mask _rapidFireMask;

    private static readonly TimeSpan JoyBufferTimeout = TimeSpan.FromMilliseconds(10);

    private DateTime _lastJoyUpdate = DateTime.MinValue;
    private int _updates;

    public void OnUpdate()
    {
        _updates++;
    }

    public uint Value
    {
        get
        {
            var secondMask = _updates % 2 == 0 ? 0 : _rapidFireMask;
            var value = (uint)(_buttonMask | secondMask) | ((uint)(byte)JoyX << 16) | ((uint)(byte)JoyY << 24);
            return value;
        }
    }

    [RelayCommand]
    private void ToggleRapidFire(Buttons.Mask bit)
    {
        _rapidFireMask = (_rapidFireMask & ~bit) | (!_rapidFireMask.HasFlag(bit) ? bit : 0);
    }

    private void SetButtonMask(Buttons.Mask bit, bool value, [CallerMemberName] string? callerMemberName = null)
    {
        // if button state is manually changed, we remove this button's rapidfire flag
        _rapidFireMask &= ~bit;

        OnPropertyChanging(callerMemberName);
        _buttonMask = (_buttonMask & ~bit) | (value ? bit : 0);
        OnPropertyChanged(callerMemberName);
    }

    public bool BtnDUp
    {
        get => (Value & (int)Buttons.Mask.DUp) != 0;
        set => SetButtonMask(Buttons.Mask.DUp, value);
    }

    public bool BtnDDown
    {
        get => (Value & (int)Buttons.Mask.DDown) != 0;
        set => SetButtonMask(Buttons.Mask.DDown, value);
    }

    public bool BtnDLeft
    {
        get => (Value & (int)Buttons.Mask.DLeft) != 0;
        set => SetButtonMask(Buttons.Mask.DLeft, value);
    }

    public bool BtnDRight
    {
        get => (Value & (int)Buttons.Mask.DRight) != 0;
        set => SetButtonMask(Buttons.Mask.DRight, value);
    }

    public bool BtnCUp
    {
        get => (Value & (int)Buttons.Mask.CUp) != 0;
        set => SetButtonMask(Buttons.Mask.CUp, value);
    }

    public bool BtnCDown
    {
        get => (Value & (int)Buttons.Mask.CDown) != 0;
        set => SetButtonMask(Buttons.Mask.CDown, value);
    }

    public bool BtnCLeft
    {
        get => (Value & (int)Buttons.Mask.CLeft) != 0;
        set => SetButtonMask(Buttons.Mask.CLeft, value);
    }

    public bool BtnCRight
    {
        get => (Value & (int)Buttons.Mask.CRight) != 0;
        set => SetButtonMask(Buttons.Mask.CRight, value);
    }

    public bool BtnA
    {
        get => (Value & (int)Buttons.Mask.A) != 0;
        set => SetButtonMask(Buttons.Mask.A, value);
    }

    public bool BtnB
    {
        get => (Value & (int)Buttons.Mask.B) != 0;
        set => SetButtonMask(Buttons.Mask.B, value);
    }

    public bool BtnStart
    {
        get => (Value & (int)Buttons.Mask.Start) != 0;
        set => SetButtonMask(Buttons.Mask.Start, value);
    }

    public bool BtnZ
    {
        get => (Value & (int)Buttons.Mask.Z) != 0;
        set => SetButtonMask(Buttons.Mask.Z, value);
    }

    public bool BtnL
    {
        get => (Value & (int)Buttons.Mask.L) != 0;
        set => SetButtonMask(Buttons.Mask.L, value);
    }

    public bool BtnR
    {
        get => (Value & (int)Buttons.Mask.R) != 0;
        set => SetButtonMask(Buttons.Mask.R, value);
    }
}