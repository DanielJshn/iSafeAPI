
using Microsoft.AspNetCore.Mvc;

namespace apitest
{
   public class NoteRepository : INoteRepository
   {

      private readonly Datadapper _dapper;

      public NoteRepository(Datadapper dapper)
      {
         _dapper = dapper;

      }

      public NoteDto AddNote(int userId, NoteDto note)
      {
         string noteSql = @"INSERT INTO dbo.Note (UserId , id , title , description , lastEdit)
                                    VALUES (@UserId, @Id ,@Title, @Description , @LastEdit)";
         var noteUUID = Guid.NewGuid();
         note.id = noteUUID;
         var noteParameters = new
         {
            UserId = userId,
            note.id,
            note.title,
            note.description,
            note.lastEdit
         };

         if (!_dapper.ExecuteSQL(noteSql, noteParameters))
         {
            throw new Exception("Failed to add Passwords");
         }
         var result = new NoteDto
         {
            id = noteParameters.id,
            title = noteParameters.title,
            description = noteParameters.description,
            lastEdit = noteParameters.lastEdit
         };
         return result;
      }

      public List<NoteResponse> GetNotes(int userId)
      {
         string sql = @"SELECT * FROM dbo.Note WHERE UserId = @userId";
         IEnumerable<NoteResponse> passwords = _dapper.LoadDatatwoParam<NoteResponse>(sql, new { userId });

         List<NoteResponse> resultPasswords = passwords.ToList();

         return resultPasswords;
      }

      public NoteDto UpdateNote(Guid noteId, NoteDto noteDto)
      {
         string updateNoteQuery = @"
                    UPDATE dbo.Note 
                    SET title = @title, description = @description, lastEdit = @lastEdit 
                    WHERE id = @id";

         var noteParameters = new
         {
            id = noteId,
            noteDto.title,
            noteDto.description,
            noteDto.lastEdit
         };

         if (!_dapper.ExecuteSQL(updateNoteQuery, noteParameters))
         {
            throw new Exception("Failed to update note");
         }

         var result = new NoteDto
         {
            id = noteId,
            title = noteDto.title,
            description = noteDto.description,
            lastEdit = noteDto.lastEdit
         };

         return result;
      }



      public void DeleteNote(Guid noteId)
      {
         
         string sqlPassword = "DELETE FROM dbo.Note WHERE id = @noteId";

         if (!_dapper.ExecuteSQL(sqlPassword, new { noteId }))
         {
            throw new Exception("Failed to delete Passwords");
         }
      }
       public void DeleteAllNote(int UserId)
      {
         
         string sqlPassword = "DELETE dbo.Note WHERE UserId = @UserID";

         if (!_dapper.ExecuteSQL(sqlPassword, new { UserId }))
         {
            throw new Exception("Failed to delete Passwords");
         }
      }


   }
}