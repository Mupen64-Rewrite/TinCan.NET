using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace TinCan.NET.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private sbyte _joyX;
    [ObservableProperty] private sbyte _joyY;
}