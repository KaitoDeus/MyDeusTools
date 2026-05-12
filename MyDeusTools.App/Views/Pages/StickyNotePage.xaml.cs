using System.Windows.Controls;
using MyDeusTools.App.ViewModels;

namespace MyDeusTools.App.Views.Pages
{
    public partial class StickyNotePage : Page
    {
        public StickyNoteViewModel ViewModel { get; }

        public StickyNotePage(StickyNoteViewModel viewModel)
        {
            ViewModel = viewModel;
            DataContext = this.ViewModel;
            InitializeComponent();
        }

        private void OnTextBoxLostFocus(object sender, System.Windows.RoutedEventArgs e)
        {
            // Tự động lưu khi người dùng gõ xong và chuyển sang chỗ khác
            if (ViewModel.AutoSaveCommand.CanExecute(null))
            {
                ViewModel.AutoSaveCommand.Execute(null);
            }
        }
    }
}
