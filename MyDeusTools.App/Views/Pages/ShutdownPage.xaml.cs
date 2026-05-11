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
    }
}
