using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace apitest
{
    [TestFixture]
    public class NoteControllerTests
    {
        private Mock<INoteRepository>? _mockNoteRepository;
        private Mock<IConfiguration>? _mockConfig;
        private NotesService? _notesService;
        private TestableNoteController? _controller;

        [SetUp]
        public void Setup()
        {
            _mockNoteRepository = new Mock<INoteRepository>();
            _mockConfig = new Mock<IConfiguration>();
            _notesService = new NotesService(_mockNoteRepository.Object);
            _controller = new TestableNoteController(_mockConfig.Object, _notesService);
        }

        [Test]
        public async Task AddNote_ShouldReturnOkResult_WhenNoteIsAdded()
        {
            // Arrange
            var noteDto = new NoteDto { title = "Test Note", description = "This is a test note" };
            var createdNoteDto = new NoteDto { id = Guid.NewGuid(), title = "Test Note", description = "This is a test note" };

            _mockNoteRepository?.Setup(r => r.AddNoteAsync(It.IsAny<int>(), It.IsAny<NoteDto>())).ReturnsAsync(createdNoteDto);

            // Act
            var result = await _controller!.AddNote(noteDto) as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result?.StatusCode);
            Assert.AreNotEqual(createdNoteDto, result?.Value); // Изменено на неверное значение
        }


        [Test]
        public async Task AddNote_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var noteDto = new NoteDto { title = "Test Note", description = "This is a test note" };

            _mockNoteRepository.Setup(r => r.AddNoteAsync(It.IsAny<int>(), It.IsAny<NoteDto>())).ThrowsAsync(new Exception("Error adding note"));

            // Act
            var result = await _controller!.AddNote(noteDto) as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result?.StatusCode);
            Assert.AreEqual("Error adding note", result?.Value);
        }

        [Test]
        public async Task GetNote_ShouldReturnOkResult_WhenNotesAreRetrieved()
        {
            // Arrange
            var notes = new List<NoteResponse>
        {
            new NoteResponse { id = Guid.NewGuid(), title = "Note 1", description = "Description 1" },
            new NoteResponse { id = Guid.NewGuid(), title = "Note 2", description = "Description 2" }
        };

            _mockNoteRepository?.Setup(r => r.GetNotesAsync(It.IsAny<int>())).ReturnsAsync(notes);

            // Act
            var result = await _controller!.GetNote() as OkObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(200, result?.StatusCode);
            Assert.AreEqual(notes, result?.Value);
        }

        [Test]
        public async Task GetNote_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            _mockNoteRepository?.Setup(r => r.GetNotesAsync(It.IsAny<int>())).ThrowsAsync(new Exception("Error retrieving notes"));

            // Act
            var result = await _controller!.GetNote() as BadRequestObjectResult;

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(400, result?.StatusCode);
            Assert.AreEqual("Error retrieving notes", result?.Value);
        }
    }
}
