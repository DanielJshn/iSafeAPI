namespace apitest
{
    public interface INoteRepository
    {
        Task<NoteDto> AddNoteAsync(int userId, NoteDto note);
        Task<List<NoteResponse>> GetNotesAsync(int userId);
        Task<NoteDto> UpdateNoteAsync(Guid noteId, NoteDto noteDto);
        Task DeleteNoteAsync(Guid noteId);
        Task DeleteAllNoteAsync(int UserId);
    }
}