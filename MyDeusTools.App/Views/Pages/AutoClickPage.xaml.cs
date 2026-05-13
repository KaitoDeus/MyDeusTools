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
            DataContext = viewModel;

            InitializeComponent();

            // Thêm sự kiện bắt phím khi Page được focus
            this.PreviewKeyDown += (s, e) => 
            {
                if (ViewModel.IsListeningForHotkey)
                {
                    // Chặn phím không cho truyền tiếp (ví dụ chặn Alt mở menu)
                    e.Handled = true;
                    
                    // Lấy các phím chức năng đang nhấn
                    var modifiers = System.Windows.Input.Keyboard.Modifiers;
                    
                    // Gọi ViewModel xử lý
                    ViewModel.ProcessCapturedKey(e.Key == System.Windows.Input.Key.System ? e.SystemKey : e.Key, modifiers);
                }
            };
        }
    }
}
