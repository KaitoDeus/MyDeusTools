using System.Windows;
using System.Windows.Input;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class ShutdownPage : System.Windows.Controls.Page
    {
        public ShutdownViewModel ViewModel { get; }

        public ShutdownPage(ShutdownViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }

        private void OnTimeInputLostFocus(object sender, RoutedEventArgs e)
        {
            ViewModel.NormalizeTime();
        }

        private void OnTimeInputKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ViewModel.NormalizeTime();
            }
        }
    }
}
