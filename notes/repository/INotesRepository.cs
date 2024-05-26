namespace apitest
{
    public interface INoteRepository
    {
        public NoteDto AddNote(int userId, NoteDto note);
        public NoteDto UpdateNote(Guid userId, NoteDto noteDto);
        public List<NoteResponse> GetNotes(int userId);
        public void DeleteNote(Guid noteId);
        public void DeleteAllNote(int UserId);
    }
}