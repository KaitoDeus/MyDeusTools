using System;
using System.Windows;
using System.Windows.Input;
using MyDeusTools.App.Services.Impl;

namespace MyDeusTools.App.Views.Windows
{
    public partial class StickyNoteWindow : Window
    {
        private readonly StickyNoteModel _note;
        private readonly IStickyNoteService _noteService;

        public StickyNoteWindow(StickyNoteModel note, IStickyNoteService noteService)
        {
            InitializeComponent();
            _note = note;
            _noteService = noteService;
            DataContext = _note;

            Closed += StickyNoteWindow_Closed;
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void StickyNoteWindow_Closed(object? sender, EventArgs e)
        {
            _note.IsPinned = false;
            await _noteService.SaveNotesAsync();
        }
    }
}
