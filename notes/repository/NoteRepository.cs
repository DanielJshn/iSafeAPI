using api.Controllers;
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

      public NoteDto UpdateNote(Guid userId, NoteDto noteDto)
      {
         string updatePasswordQuery = @"
                        UPDATE dbo.Note 
                        SET title = @title, description =  @description , lastEdit = @LastEdit WHERE id = @id";

         var passwordParameters = new
         {
            userId,
            noteDto.title,
            noteDto.description,
            noteDto.lastEdit
         };

         if (!_dapper.ExecuteSQL(updatePasswordQuery, passwordParameters))
         {
            throw new Exception("Failed to update Passwords");
         }

         var result = new NoteDto();
         {
            result.id = userId;
            result.title = noteDto.title;
            result.description = noteDto.description;
            result.lastEdit = noteDto.lastEdit;
         };
         return result;
      }


      public void DeleteNote(Guid noteId)
      {

         string sqlPassword = "DELETE FROM dbo.Note WHERE id = @id";

         if (!_dapper.ExecuteSQL(sqlPassword, new { noteId }))
         {
            throw new Exception("Failed to delete Passwords");
         }
      }

   }
}