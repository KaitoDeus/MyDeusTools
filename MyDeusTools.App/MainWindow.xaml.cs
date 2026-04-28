using System.Windows;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindowViewModel ViewModel { get; }

    public MainWindow(MainWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = ViewModel; // Kết nối View và ViewModel

        InitializeComponent();
    }
}