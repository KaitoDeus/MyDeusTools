using CommunityToolkit.Mvvm.ComponentModel;
using Wpf.Ui;

namespace MyDeusTools.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly INavigationService _navigationService;

    public MainWindowViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
    }
}