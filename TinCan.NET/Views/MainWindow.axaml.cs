using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using TinCan.NET.Helpers;
using TinCan.NET.ViewModels;

namespace TinCan.NET.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        AvaloniaXamlLoader.Load(this);
#if DEBUG
        this.AttachDevTools();
#endif
        AddHandler(PointerReleasedEvent, Window_OnPointerReleasedFilter, RoutingStrategies.Bubble, handledEventsToo: true);
    }

    public MainWindowViewModel ViewModel => (DataContext as MainWindowViewModel)!;


    private async void Window_OnClosing(object? sender, WindowClosingEventArgs e)
    {
        // this attempts to ping the host
        if (e.IsProgrammatic)
            return;
        e.Cancel = true;
        if (!await ((App) Application.Current!).PingHost())
            Close();
    }

    private void Window_OnPointerReleasedFilter(object? sender, PointerReleasedEventArgs e)
    {
        if (FocusManager == null)
            return;
        var focusSource = FocusManager.GetFocusedElement();
        if (focusSource is TextBox)
            return;
        
        ((App) Application.Current!).ActivateMainWindow();
    }

    private void Button_OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var control = sender as Avalonia.Controls.Control;
        if (e.Pointer.Type != PointerType.Mouse) return;
        if (!e.GetCurrentPoint(null).Properties.IsRightButtonPressed)
            return;

        int flag = int.Parse((string)(control!.Tag!));
        ViewModel.ToggleRapidFireCommand.Execute((Buttons.Mask)flag);
    }
}