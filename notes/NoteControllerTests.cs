using NUnit.Framework;
using Moq;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework.Legacy;
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
        public void AddNote_ShouldReturnOkResult_WhenNoteIsAdded()
        {
            // Arrange
            var noteDto = new NoteDto { title = "Test Note", description = "This is a test note" };
            var createdNoteDto = new NoteDto { id = Guid.NewGuid(), title = "Test Note", description = "This is a test note" };

            _mockNoteRepository?.Setup(r => r.AddNote(It.IsAny<int>(), It.IsAny<NoteDto>())).Returns(createdNoteDto);

            // Act
            var result = _controller.AddNote(noteDto) as OkObjectResult;

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(200, result?.StatusCode);
            ClassicAssert.AreEqual(createdNoteDto, result?.Value);
        }

        [Test]
        public void AddNote_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            var noteDto = new NoteDto { title = "Test Note", description = "This is a test note" };

            _mockNoteRepository.Setup(r => r.AddNote(It.IsAny<int>(), It.IsAny<NoteDto>())).Throws(new Exception("Error adding note"));

            // Act
            var result = _controller.AddNote(noteDto) as BadRequestObjectResult;

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(400, result?.StatusCode);
            ClassicAssert.AreEqual("Error adding note", result?.Value);
        }
        [Test]
        public void GetNote_ShouldReturnOkResult_WhenNotesAreRetrieved()
        {
            // Arrange
            var notes = new List<NoteResponse>
            {
                new NoteResponse { id = Guid.NewGuid(), title = "Note 1", description = "Description 1" },
                new NoteResponse { id = Guid.NewGuid(), title = "Note 2", description = "Description 2" }
            };

            _mockNoteRepository?.Setup(r => r.GetNotes(It.IsAny<int>())).Returns(notes);

            // Act
            var result = _controller?.GetNote() as OkObjectResult;

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(200, result?.StatusCode);
            ClassicAssert.AreEqual(notes, result?.Value);
        }

        [Test]
        public void GetNote_ShouldReturnBadRequest_WhenExceptionIsThrown()
        {
            // Arrange
            _mockNoteRepository?.Setup(r => r.GetNotes(It.IsAny<int>())).Throws(new Exception("Error retrieving notes"));

            // Act
            var result = _controller?.GetNote() as BadRequestObjectResult;

            // Assert
            ClassicAssert.IsNotNull(result);
            ClassicAssert.AreEqual(400, result?.StatusCode);
            ClassicAssert.AreEqual("Error retrieving notes", result?.Value);
        }
    }
    }

    