namespace apitest
{
    public class TestableNoteController : NoteController
    {
        public TestableNoteController(IConfiguration config, NotesService notesService)
            : base(config, notesService)
        {
        }

        protected override int getUserId()
        {
            // Возвращаем фиксированное значение для тестов
            return 1;
        }
    }
}