using Microsoft.IdentityModel.Tokens;

namespace apitest
{

    public class NotesService : INotesService
    {
        private readonly INoteRepository _noteRepository;

        public NotesService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public async Task<NoteDto> AddNoteAsync(int userId, NoteDto note)
        {
            if (note == null)
            {
                throw new Exception("Note data is null!");
            }
            if (string.IsNullOrEmpty(note.title))
            {
                throw new Exception("Title is empty!");
            }
            if (string.IsNullOrEmpty(note.description))
            {
                throw new Exception("Description is empty!");
            }
            return await _noteRepository.AddNoteAsync(userId, note);
        }

        public async Task<List<NoteResponse>> GetNotesAsync(int userId)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }
            return await _noteRepository.GetNotesAsync(userId);
        }

        public async Task<NoteDto> UpdateNoteAsync(Guid noteID, NoteDto noteDto)
        {
            if (noteDto == null)
            {
                throw new Exception("Note data is null");
            }
            if (noteID == Guid.Empty)
            {
                throw new Exception("Note not found");
            }
            if (string.IsNullOrEmpty(noteDto.title))
            {
                throw new Exception("Title is empty!");
            }
            if (string.IsNullOrEmpty(noteDto.description))
            {
                throw new Exception("Description is empty!");
            }
            return await _noteRepository.UpdateNoteAsync(noteID, noteDto);
        }

        public async Task DeleteNoteAsync(Guid noteID)
        {
            if (noteID == Guid.Empty)
            {
                throw new Exception("Note not found");
            }
            await _noteRepository.DeleteNoteAsync(noteID);
        }

        public async Task DeleteAllNoteAsync(int UserId)
        {
            if (UserId == 0)
            {
                throw new Exception("User not found");
            }
            await _noteRepository.DeleteAllNoteAsync(UserId);
        }
    }

}