using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
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
}