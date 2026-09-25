using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class TextUtilityPage : Page
    {
        public TextUtilityViewModel ViewModel { get; }

        public TextUtilityPage(TextUtilityViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
