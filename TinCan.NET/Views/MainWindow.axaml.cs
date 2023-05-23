using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using TinCan.NET.ViewModels;

namespace TinCan.NET.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            AvaloniaXamlLoader.Load(this);
            DataContext = new MainWindowViewModel();
#if DEBUG
            this.AttachDevTools();
#endif
        }
    }
}