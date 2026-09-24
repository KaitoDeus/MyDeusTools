using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class ColorPickerPage : Page
    {
        public ColorPickerViewModel ViewModel { get; }

        public ColorPickerPage(ColorPickerViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = viewModel;

            InitializeComponent();
        }
    }
}
