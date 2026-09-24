using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class ClipboardPage : Page
    {
        public ClipboardViewModel ViewModel { get; }

        public ClipboardPage(ClipboardViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
