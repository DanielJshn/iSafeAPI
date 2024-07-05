using Microsoft.IdentityModel.Tokens;

namespace apitest
{

    public class NotesService
    {

        private readonly INoteRepository _noteRepository;

        public NotesService(INoteRepository noteRepository)
        {
            _noteRepository = noteRepository;
        }

        public NoteDto AddNote(int userId, NoteDto note)
        {
            if (note.title.IsNullOrEmpty())
            {
                throw new Exception("Title is empty!");
            }
            if (note.description.IsNullOrEmpty())
            {
                throw new Exception("Description is empty!");
            }
            return _noteRepository.AddNote(userId, note);
        }

        public List<NoteResponse> GetNotes(int userId)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }
            return _noteRepository.GetNotes(userId);
        }

        public NoteDto UpdateNote(Guid noteID, NoteDto noteDto)
        {
            if (noteID == Guid.Empty)
            {
                throw new Exception("Note not found");
            }
            if (noteDto.title.IsNullOrEmpty())
            {
                throw new Exception("Title is empty!");
            }
            if (noteDto.description.IsNullOrEmpty())
            {
                throw new Exception("Description is empty!");
            }
            return _noteRepository.UpdateNote(noteID, noteDto);
        }


        public void DeleteNote(Guid noteID)
        {
            if (noteID == Guid.Empty)
            {
                throw new Exception("Note not found");
            }
            _noteRepository.DeleteNote(noteID);
        }
        public void DeleteAllNote(int UserId)
        {
            if (UserId == 0)
            {
                throw new Exception("User not found");
            }
             _noteRepository.DeleteAllNote(UserId);
        }

    }

}