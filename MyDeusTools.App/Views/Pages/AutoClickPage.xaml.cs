using MyDeusTools.App.ViewModels;
using Wpf.Ui.Controls;

namespace MyDeusTools.App.Views.Pages
{
    public partial class AutoClickPage : System.Windows.Controls.Page
    {
        public AutoClickViewModel ViewModel { get; }

        public AutoClickPage(AutoClickViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel; // Sửa từ 'this' thành 'viewModel'

            InitializeComponent();
        }
    }
}
