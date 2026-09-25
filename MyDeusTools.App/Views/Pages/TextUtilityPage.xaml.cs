using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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

        private void OnScrollViewerPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (sender is not ScrollViewer scrollViewer) return;

            // If the mouse pointer is directly over a multi-line TextBox, check if it can scroll internally
            if (e.OriginalSource is DependencyObject dep)
            {
                var textBox = FindAncestor<TextBox>(dep);
                if (textBox != null && textBox.AcceptsReturn)
                {
                    bool canScrollDown = e.Delta < 0 && textBox.VerticalOffset < (textBox.ExtentHeight - textBox.ViewportHeight - 0.001);
                    bool canScrollUp = e.Delta > 0 && textBox.VerticalOffset > 0.001;

                    if (canScrollDown || canScrollUp)
                    {
                        return; // Let the TextBox scroll its internal text
                    }
                }
            }

            scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
        {
            while (current != null)
            {
                if (current is T match) return match;
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }
    }
}
