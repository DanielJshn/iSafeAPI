using api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest;
[Authorize]
[ApiController]
[Route("[controller]")]
public class PasswordController : ControllerBase
{
    Datadapper _dapper;
    private readonly ILogger<UserController> _logger;

    IConfiguration _config;

    public PasswordController(IConfiguration config, ILogger<UserController> logger)
    {
        _dapper = new Datadapper(config);
        _config = config;
        _logger = logger;
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }


    [HttpGet("GetPassword")]
    public IEnumerable<Passwords> GetPasswords()
    {
        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length);
        }
        accessToken = accessToken?.Trim();
        int userId = 0;

        string sql0 = @"SELECT UserId From dbo.Tokens Where TokenValue= '" + accessToken + "'";
        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(sql0, conn);
            object result = command.ExecuteScalar();
            if (result != null)
            {
                userId = Convert.ToInt32(result);
            }
            conn.Close();
        }


        string sql = @"SELECT * FROM dbo.[Passwords] WHERE UserId =" + userId;
        IEnumerable<Passwords> passwords = _dapper.LoadData<Passwords>(sql);

        List<Passwords> resultPasswords = new List<Passwords>();

        foreach (var password in passwords)
        {
            string sql2 = @"SELECT * FROM dbo.AdditionalFields WHERE passwordId =" + password.id;
            IEnumerable<AdditionalField> additionalFields = _dapper.LoadData<AdditionalField>(sql2);
            password.additionalFields = additionalFields.ToList();
            resultPasswords.Add(password);
        }
        return resultPasswords;




    }

    [HttpPost("AddPassword")]
    public IActionResult AddPassword([FromBody] Passwords userInput)
    {
        if (userInput == null || userInput.additionalFields == null)
        {
            return BadRequest("Invalid input data");
        }
        int userId = getUserId();


        if (userId == 0)
        {
            return BadRequest("Invalid or missing UserId");
        }

        string passwordSql = @" INSERT INTO dbo.Passwords ([UserId], [organization], [title])
        VALUES
        ('" + userId +
          "', '" + userInput.organization +
          "', '" + userInput.title + "');";

        if (!_dapper.ExecuteSQL(passwordSql))
        {
            throw new Exception("Failed to add Passwords");
        }
        string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";

        int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();

        foreach (var additionalField in userInput.additionalFields)
        {
            additionalField.passwordId = insertedPasswordId; // Используем полученный ID из таблицы Passwords

            string additionalFieldSql = @"
        INSERT INTO dbo.AdditionalFields (passwordId, title_, [value])  
        VALUES (@passwordId, @title, @value)";

            var fieldParameters = new
            {
                passwordId = additionalField.passwordId,
                title = additionalField.title_,
                value = additionalField.value
            };

            if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
            {
                throw new Exception("Failed to add AdditionalField");
            }
        }

        return Ok();

    }
    [HttpPut("UpdatePassword/{id}")]
    public IActionResult UpdatePassword([FromBody] Passwords userInput, int id)
    {



        // Проверка наличия UserId и входных данных
        if (id == 0 || userInput == null)
        {
            return BadRequest("Invalid input data or user information");
        }

        // Обновление информации в таблице Passwords
        string updatePasswordQuery = @"
        UPDATE dbo.Passwords 
        SET organization = @organization, title = @title 
        WHERE id = @id";

        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(updatePasswordQuery, conn);
            command.Parameters.AddWithValue("@organization", userInput.organization ?? "");
            command.Parameters.AddWithValue("@title", userInput.title ?? "");
            command.Parameters.AddWithValue("@id", id);

            int rowsAffected = command.ExecuteNonQuery();

            if (rowsAffected < 1)
            {
                Console.WriteLine("Failed to update Passwords");
                return StatusCode(500, "Failed to update Passwords");
            }

            Console.WriteLine("Passwords updated successfully");
            conn.Close();
        }

        string deleteAdditionalFieldQuery = @"
                DELETE FROM dbo.AdditionalFields 
                WHERE passwordId =" + id.ToString();

        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(deleteAdditionalFieldQuery, conn);

            int deletedRows = command.ExecuteNonQuery();

            if (deletedRows < 0)
            {
                Console.WriteLine("Failed to delete existing AdditionalField entries");
                return StatusCode(500, "Failed to delete existing AdditionalField entries");
            }

            Console.WriteLine("All AdditionalField entries deleted successfully");
            conn.Close();
        }
        string getPasswordIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";

        int currentPasswordId = 0; // Инициализируем текущий passwordId

        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand getPasswordIdCommand = new SqlCommand(getPasswordIdQuery, conn);

            object result = getPasswordIdCommand.ExecuteScalar();

            if (result != null && int.TryParse(result.ToString(), out int retrievedPasswordId))
            {
                currentPasswordId = retrievedPasswordId;
            }
            else
            {
                // Обработка ошибки, если не удалось получить текущий passwordId
            }

            conn.Close();
        }

        // Добавление новых данных в таблицу AdditionalFields с использованием полученного passwordId
        foreach (var additionalField in userInput.additionalFields)
        {
            string insertAdditionalFieldQuery = @"
        INSERT INTO dbo.AdditionalFields (passwordId, title_, [value])  
        VALUES (@passwordId, @title, @value)";

            var fieldParameters = new
            {
                passwordId = currentPasswordId, // Используем текущий passwordId
                title = additionalField.title_,
                value = additionalField.value
            };

            // Выполнение запроса на добавление данных
            if (!_dapper.ExecuteSQL(insertAdditionalFieldQuery, fieldParameters))
            {
                throw new Exception("Failed to add AdditionalField");
            }
        }

        return Ok();

    }



    [HttpDelete("DeletePassword/{id}")]
    public IActionResult DeletePassword(int id)
    {
        try
        {
            string sqlPassword = @"DELETE from dbo.Passwords WHERE id= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlPassword))
            {
                _logger.LogError($"Failed to delete user password for ID: {id}");
                return StatusCode(500, "Failed to delete user password");
            }

            string sqlAdditional = @"DELETE from dbo.AdditionalFields WHERE passwordId= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlAdditional))
            {
                _logger.LogError($"Failed to delete user additional fields for ID: {id}");
                return StatusCode(500, "Failed to delete user additional fields");
            }

            _logger.LogInformation($"User with ID {id} successfully deleted");
            return Ok("User successfully deleted");
        }
        catch (Exception ex)
        {
            // Logging detailed error information
            _logger.LogError($"Error while deleting user with ID {id}: {ex.Message}");
            return StatusCode(500, "An error occurred while deleting the user");
        }
    }
    private int getUserId()
    {
        string? accessToken = HttpContext.Request.Headers["Authorization"];
        if (accessToken != null && accessToken.StartsWith("Bearer "))
        {
            accessToken = accessToken.Substring("Bearer ".Length);
        }
        accessToken = accessToken?.Trim();
        int userId = 0;

        string sql0 = @"SELECT UserId From dbo.Tokens Where TokenValue= '" + accessToken + "'";
        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand command = new SqlCommand(sql0, conn);
            object result = command.ExecuteScalar();
            if (result != null)
            {
                userId = Convert.ToInt32(result);
                return userId;
            }
            conn.Close();
        }
        throw new Exception("Can't get user id");
    }

}