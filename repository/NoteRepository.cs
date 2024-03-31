using api.Controllers;

namespace apitest
{
   public class NoteRepository
   {

      private readonly Datadapper _dapper;
      public NoteRepository(Datadapper dapper)
      {
         _dapper = dapper;
      }
      public NoteDto PostNote(int userId, NoteDto userInput)
      {
         string noteSql = @"INSERT INTO dbo.Note (UserId , id , title , description , lastEdit)
                                    VALUES (@UserId, @Id ,@Title, @Description , @LastEdit)";
          var noteUUID = Guid.NewGuid();
            userInput.id = noteUUID;
         var noteParameters = new
         {
            UserId = userId,
            userInput.id,
            userInput.title,
            userInput.description,
            userInput.lastEdit
         };

         if (!_dapper.ExecuteSQL(noteSql, noteParameters))
         {
            throw new Exception("Failed to add Passwords");
         }
         var result = new NoteDto();
         result.id = noteParameters.id;
         result.title = noteParameters.title;
         result.description = noteParameters.description;
         result.lastEdit = noteParameters.lastEdit;
         return result;
      }

      public List<NoteResponse> getAllNotes(int userId)
      {

         string sql = @"SELECT * FROM dbo.Note WHERE UserId = @userId";
         IEnumerable<NoteResponse> passwords = _dapper.LoadDatatwoParam<NoteResponse>(sql, new { userId });

         List<NoteResponse> resultPasswords = passwords.ToList();

         return resultPasswords;
      }

      public void UpdateNote(Guid id, NoteDto userInput)
      {
         string updatePasswordQuery = @"
                        UPDATE dbo.Note 
                        SET title = @title, description =  @description , lastEdit = @LastEdit WHERE id = @id";
              
         var passwordParameters = new
         {
             id,
            userInput.title,
            userInput.description,
            userInput.lastEdit
         };

         if (!_dapper.ExecuteSQL(updatePasswordQuery, passwordParameters))
         {
            throw new Exception("Failed to update Passwords");
         }


      }
      public void DeleteData(Guid id)
      {

         string sqlPassword = "DELETE FROM dbo.Note WHERE id = @id";

         if (!_dapper.ExecuteSQL(sqlPassword, new { id }))
         {
            throw new Exception("Failed to delete Passwords");
         }
      }
   }
}