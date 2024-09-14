namespace apitest
{
    public interface INotesService
    {
        Task<NoteDto> AddNoteAsync(int userId, NoteDto note);
        Task<List<NoteResponse>> GetNotesAsync(int userId);
        Task<NoteDto> UpdateNoteAsync(Guid noteID, NoteDto noteDto);
        Task DeleteNoteAsync(Guid noteID);
        Task DeleteAllNoteAsync(int UserId);
    }
}
