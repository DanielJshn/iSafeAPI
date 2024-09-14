
using Microsoft.AspNetCore.Mvc;

namespace apitest
{
   public class NoteRepository : INoteRepository
   {
      private readonly DatadapperAsync _dapper;

      public NoteRepository(DatadapperAsync dapper)
      {
         _dapper = dapper;
      }

      public async Task<NoteDto> AddNoteAsync(int userId, NoteDto note)
      {
         string noteSql = @"INSERT INTO dbo.Note (UserId , id , title , description , lastEdit)
                                    VALUES (@UserId, @id , @title, @description , @lastEdit)";
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

         var result = await _dapper.ExecuteSQLAsync(noteSql, noteParameters);
         if (!result)
         {
            throw new Exception("Failed to add Passwords");
         }
         var addedNote = new NoteDto
         {
            id = noteParameters.id,
            title = noteParameters.title,
            description = noteParameters.description,
            lastEdit = noteParameters.lastEdit
         };
         return addedNote;
      }

      public async Task<List<NoteResponse>> GetNotesAsync(int userId)
      {
         string sql = @"SELECT * FROM dbo.Note WHERE UserId = @userId";
         IEnumerable<NoteResponse> notes = await _dapper.LoadDatatwoParamAsync<NoteResponse>(sql, new { userId });

         return notes.ToList();
      }

      public async Task<NoteDto> UpdateNoteAsync(Guid noteId, NoteDto noteDto)
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

         var result = await _dapper.ExecuteSQLAsync(updateNoteQuery, noteParameters);
         if (!result)
         {
            throw new Exception("Failed to update note");
         }

         var updatedNote = new NoteDto
         {
            id = noteId,
            title = noteDto.title,
            description = noteDto.description,
            lastEdit = noteDto.lastEdit
         };

         return updatedNote;
      }

      public async Task DeleteNoteAsync(Guid noteId)
      {
         string sqlPassword = "DELETE FROM dbo.Note WHERE id = @noteId";
         await _dapper.ExecuteSQLAsync(sqlPassword, new { noteId });
      }

      public async Task DeleteAllNoteAsync(int UserId)
      {
         string sqlPassword = "DELETE dbo.Note WHERE UserId = @UserID";
         await _dapper.ExecuteSQLAsync(sqlPassword, new { UserId });
      }
   }
}