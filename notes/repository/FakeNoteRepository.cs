
namespace apitest
{

    class FakeNoteRepository : INoteRepository
    {

        private List<NoteDto> _notes = new List<NoteDto>();

        public NoteDto AddNote(int userId, NoteDto note)
        {
            note.id = Guid.NewGuid();
            _notes.Add(note);
            return note;
        }

        public void DeleteNote(Guid noteId)
        {
            _notes.RemoveAll(n => n.id == noteId);
        }

        public List<NoteResponse> GetNotes(int userId)
        {
            List<NoteResponse> noteResponses = new List<NoteResponse>();
            foreach (NoteDto note in _notes)
            {
                noteResponses.Add(
                    new NoteResponse
                    {
                        id = note.id,
                        title = note.title,
                        description = note.description,
                        lastEdit = note.lastEdit
                    }
                );
            }
            return noteResponses;

        }

        public NoteDto UpdateNote(Guid noteId, NoteDto noteDto)
        {
            var note = _notes.FirstOrDefault(n => n.id == noteId);
            if (note != null)
            {
                // Обновляем данные заметки из объекта NoteDto
                note.title = noteDto.title;
                note.description = noteDto.description;
                note.lastEdit = noteDto.lastEdit;
            }
            return note;
        }
    }


}