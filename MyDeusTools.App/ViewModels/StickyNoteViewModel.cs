using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyDeusTools.App.Services.Impl;
using System.Linq;

namespace MyDeusTools.App.ViewModels
{
    public partial class StickyNoteViewModel : ObservableObject
    {
        private readonly IStickyNoteService _noteService;

        private ObservableCollection<StickyNoteModel> _notes;
        public ObservableCollection<StickyNoteModel> Notes
        {
            get => _notes;
            set => SetProperty(ref _notes, value);
        }

        public StickyNoteViewModel(IStickyNoteService noteService)
        {
            _noteService = noteService;
            _notes = _noteService.GetNotes();
        }

        [RelayCommand]
        private void AddNote()
        {
            var newNote = new StickyNoteModel
            {
                Content = "Ghi chú mới...",
                CreatedAt = System.DateTime.Now
            };
            _noteService.AddNote(newNote);
        }

        [RelayCommand]
        private void DeleteNote(StickyNoteModel note)
        {
            if (note != null)
            {
                _noteService.RemoveNote(note);
            }
        }

        [RelayCommand]
        private void PinNote(StickyNoteModel note)
        {
            if (note != null)
            {
                note.IsPinned = true;
                _ = _noteService.SaveNotesAsync();
                var window = new Views.Windows.StickyNoteWindow(note, _noteService);
                window.Show();
            }
        }

        [RelayCommand]
        private async Task AutoSave()
        {
            await _noteService.SaveNotesAsync();
        }
    }
}
