using System.Windows.Controls;
using System.Windows.Input;
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

        private void OnScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is ScrollViewer scrollViewer)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
                e.Handled = true;
            }
        }
    }
}
