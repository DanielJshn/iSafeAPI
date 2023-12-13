using System.Data;
using api.Controllers;
using apitest;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace apitest;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
   Datadapper _dapper;
private readonly ILogger<UserController> _logger;

   IConfiguration _config;

     public UserController(IConfiguration config,ILogger<UserController> logger)
    {
       _dapper=new Datadapper(config);
       _config = config;
        _logger = logger;
    }



[HttpGet("GetUser/{id}")]
    public Passwords GetPasswords(int id)
{
            var authHeader = Request.Headers[HeaderNames.Authorization];
            if (authHeader != null && authHeader.StartsWith("Bearer "))
                 {
                  // Извлечение токена из заголовка Authorization
                  string accessToken = authHeader.Substring("Bearer ".Length).Trim();
                  Console.WriteLine(ac)

            // Дальнейшая обработка токена...
            
            // Вернуть успешный результат или выполнить необходимые действия с токеном
            } 
            else 
            {
                  throw new Exception("AuthToken is null");
            }
    string sql = @"
        SELECT * from dbo.[Passwords] Where id ="+ id.ToString();
        Passwords passwords = _dapper.LoadDataSingle<Passwords>(sql);

         string sql2 = @"
       SELECT * from dbo.AdditionalFields Where passwordId  ="+ id.ToString();
        IEnumerable<AdditionalField> additionalField = _dapper.LoadData<AdditionalField>(sql2);

        passwords.additionalFields = additionalField.AsList();

 
        return passwords;
}
  
[HttpPost("AddUser")]
    public IActionResult AddUser([FromBody] Passwords userInput)

    {
        if (userInput == null || userInput.additionalFields == null)
        {
            return BadRequest("Invalid input data");
        }

      string passwordSql = @"
    INSERT INTO dbo.Passwords (
        [organization],
        [title]
    ) VALUES (
        '" + userInput.organization +
        "', '" + userInput.title + "');";


      
        if (!_dapper.ExecuteSQL(passwordSql))
        {
            throw new Exception("Failed to add Passwords");
        }
        string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";

        int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();


        foreach (var additionalField in userInput.additionalFields)
        {
         string additionalFieldSql = @"
    INSERT INTO dbo.AdditionalFields (
        passwordId,
        title_,
        [value]
    ) VALUES (
        " + insertedPasswordId + ", '" +
        additionalField.title_ +
        "', '" + additionalField.value + "')";


            if (!_dapper.ExecuteSQL(additionalFieldSql))
            {
                throw new Exception("Failed to add AdditionalField");
            }
       
        }

        return Ok();
    }

[HttpPut("UpdateUser")]
    public IActionResult UpdateUser([FromBody] Passwords userInput, int id)
        {
            if (userInput == null || userInput.additionalFields == null)
            {
                return BadRequest("Invalid input data");
            }

            string passwordSql = @"
                UPDATE dbo.Passwords
                SET 
                organization = @org,
                title = @title
                WHERE id = @id";

            var passwordParameters = new { org = userInput.organization, title = userInput.title, id };

            if (!_dapper.ExecuteSQL(passwordSql, passwordParameters))
            {
                throw new Exception("Failed to update entry in Passwords table");
            }

          
                  string sqlAdditional = @"DELETE from dbo.AdditionalFields WHERE passwordId= " + id.ToString();

               
                if (!_dapper.ExecuteSQL(sqlAdditional))
                {
                    throw new Exception("Failed to update entry in  AdditionalFields table");
                }
                    string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";

                int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();

                 foreach (var additionalField in userInput.additionalFields)
                        {
                        string additionalFieldSql = @"
                            INSERT INTO dbo.AdditionalFields (
                                passwordId,
                                title_,
                                [value]
                            ) VALUES (
                                " + insertedPasswordId + ", '" +
                                additionalField.title_ +
                                "', '" + additionalField.value + "')";


            if (!_dapper.ExecuteSQL(additionalFieldSql))
            {
                throw new Exception("Failed to add AdditionalField");
            }
            }

            return Ok();
        }

[HttpDelete("DeleteUser/{id}")]
    public IActionResult DeleteUser([FromBody] Passwords userInput, int id)
    {
        if (userInput == null || userInput.additionalFields == null)
        {
            return BadRequest("Invalid input data");
        }

        try
        {
            string sqlPassword = @"DELETE from dbo.Passwords WHERE timestamp_= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlPassword))
            {
                return StatusCode(500, "Failed to delete user password");
            }

            string sqlAdditional = @"DELETE from dbo.AdditionalFields WHERE timestamp_= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlAdditional))
            {
                return StatusCode(500, "Failed to delete user additional fields");
            }

            return Ok("User successfully deleted");
        }
        catch (Exception ex)
        {
            // Logging detailed error information
            _logger.LogError($"Error while deleting user: {ex.Message}");
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }

[HttpGet("GetUserData/{userId}")]
    public UserData GetUserData(int userId)
    {
    string sql =@"SELECT * FROM dbo.UserData where userId ="+ userId.ToString();
    
    UserData userData = _dapper.LoadDataSingle<UserData>(sql);

    return userData;

    }

[HttpPost("AddUserData")]
    public IActionResult AddUSerData(UserData userData)
    {
    string sql =@"INSERT INTO dbo.UserData(
        
        Name,
        UserSecret
        )VALUES(
        '" + userData.Name +
        "', '" + userData.UserSecret + "')";
        
        if (!_dapper.ExecuteSQL(sql))
        {
            throw new Exception("Failed to add UserData");
        }
        return Ok();
    }

[HttpDelete("DeleteUserData/{userId}")]
public IActionResult DeleteUser(int userId)
{
    string sql = "DELETE FROM dbo.UserData WHERE UserId = @UserId";
    var parameters = new { UserId = userId };

    if (!_dapper.ExecuteSQL(sql, parameters))
    {
        return NoContent(); 
    }

    return NotFound(); 
}


            
   
  
}


