using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class WindowPinnerPage : Page
    {
        public WindowPinnerViewModel ViewModel { get; }

        public WindowPinnerPage(WindowPinnerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
