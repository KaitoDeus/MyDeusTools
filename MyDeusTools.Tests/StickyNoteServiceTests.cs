using System.Collections.ObjectModel;
using MyDeusTools.App.Services.Impl;
using Xunit;

namespace MyDeusTools.Tests
{
    public class StickyNoteServiceTests
    {
        [Fact]
        public async Task AddNote_ShouldSaveNoteToFile()
        {
            // Arrange
            var service = new StickyNoteService();
            var initialCount = service.GetNotes().Count;
            var newNote = new StickyNoteModel { Content = "Test Note" };

            // Act
            service.AddNote(newNote);
            await service.SaveNotesAsync();

            // Assert
            var notes = service.GetNotes();
            Assert.Contains(notes, n => n.Content == "Test Note");
            Assert.Equal(initialCount + 1, notes.Count);
        }

        [Fact]
        public void LoadNotes_ShouldHandleMissingFile()
        {
            // Arrange
            // Đảm bảo môi trường sạch (xóa file nếu có - trong thực tế nên dùng Mock hoặc thư mục tạm)
            var service = new StickyNoteService();
            
            // Act
            service.LoadNotes();

            // Assert
            Assert.NotNull(service.GetNotes());
        }
    }
}
