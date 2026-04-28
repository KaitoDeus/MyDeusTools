using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui;
using Wpf.Ui.Mvvm.Contracts;

namespace MyDeusTools.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private string _title = "MyDeusTools - Siêu ứng dụng";

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}