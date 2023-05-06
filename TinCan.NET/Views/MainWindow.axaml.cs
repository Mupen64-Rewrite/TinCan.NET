using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TinCan.NET.ViewModels;

namespace TinCan.NET.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
#if DEBUG
#endif
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
        DataContext = new MainWindowViewModel();
    }
}