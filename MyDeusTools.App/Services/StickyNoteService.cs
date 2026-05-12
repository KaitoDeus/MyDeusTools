using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace MyDeusTools.App.Services.Impl
{
    public interface IStickyNoteService
    {
        ObservableCollection<StickyNoteModel> GetNotes();
        void AddNote(StickyNoteModel note);
        void RemoveNote(StickyNoteModel note);
        Task SaveNotesAsync();
        void LoadNotes();
    }

    public partial class StickyNoteModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        private string _content = string.Empty;
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public string Color { get; set; } = "#FEFF9C"; // Default yellow
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsPinned { get; set; } = false;
    }

    public class StickyNoteService : IStickyNoteService
    {
        private readonly string _filePath;
        private ObservableCollection<StickyNoteModel> _notes = new();

        public StickyNoteService()
        {
            // string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            // string folder = Path.Combine(appData, "MyDeusTools");
            string folder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
            if (!Directory.Exists(folder)) Directory.CreateDirectory(folder);
            _filePath = Path.Combine(folder, "notes.json");

            LoadNotes();
        }

        public ObservableCollection<StickyNoteModel> GetNotes() => _notes;

        public void AddNote(StickyNoteModel note)
        {
            _notes.Add(note);
            _ = SaveNotesAsync();
        }

        public void RemoveNote(StickyNoteModel note)
        {
            if (_notes.Remove(note))
            {
                _ = SaveNotesAsync();
            }
        }

        public async Task SaveNotesAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                string json = JsonSerializer.Serialize(_notes, options);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi lưu ghi chú: {ex.Message}");
            }
        }

        public void LoadNotes()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string json = File.ReadAllText(_filePath);
                    var loadedNotes = JsonSerializer.Deserialize<List<StickyNoteModel>>(json);
                    if (loadedNotes != null)
                    {
                        _notes = new ObservableCollection<StickyNoteModel>(loadedNotes);
                    }
                }
                else
                {
                    // Thêm ghi chú mặc định nếu chưa có gì
                    _notes = new ObservableCollection<StickyNoteModel>();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi khi đọc ghi chú: {ex.Message}");
                _notes = new ObservableCollection<StickyNoteModel>();
            }
        }
    }
}
