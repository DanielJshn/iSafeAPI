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
    public IActionResult GetPasswords()
    {
        try
        {
            getUserId();
        }
        catch (Exception ex)
        {
            return StatusCode(401, ex.Message);
        }
        int userId = getUserId();

        if (userId == 0)
        {
            return BadRequest("Неверный или отсутствующий идентификатор пользователя");
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

        if (resultPasswords == null || !resultPasswords.Any())
        {
            return NotFound("Пароли для указанного идентификатора пользователя не найдены");
        }

        return Ok(resultPasswords);
    }






    [HttpPost("AddPassword")]
    public IActionResult AddPassword([FromBody] PasswordDto userInput)
    {
        try
        {
            getUserId();
        }
        catch (Exception ex)
        {
            return StatusCode(401, ex.Message);
        }
        if (userInput == null || userInput.additionalFields == null)
        {
            return BadRequest("Invalid input data");
        }

        int userId = getUserId();

        if (userId == 0)
        {
            return BadRequest("Invalid or missing UserId");
        }

        string passwordSql = @"INSERT INTO dbo.Passwords ([UserId], [password], [organization], [title])
                        VALUES (@UserId, @Password, @Organization, @Title);";

        var passwordParameters = new
        {
            UserId = userId,
            userInput.password,
            userInput.organization,
            userInput.title
        };

        if (!_dapper.ExecuteSQL(passwordSql, passwordParameters))
        {
            throw new Exception("Failed to add Passwords");
        }

        string getIdQuery = "SELECT TOP 1 id FROM dbo.Passwords ORDER BY id DESC";
        int insertedPasswordId = _dapper.LoadData<int>(getIdQuery).FirstOrDefault();
        foreach (var additionalField in userInput.additionalFields)
        {
            string additionalFieldSql = @"
        INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
        VALUES (@passwordId, @title, @value)";

            var fieldParameters = new
            {
                passwordId = insertedPasswordId,
                title = additionalField.title,
                value = additionalField.value
            };

            if (!_dapper.ExecuteSQL(additionalFieldSql, fieldParameters))
            {
                throw new Exception("Failed to add AdditionalField");
            }
        }

        // Проверяем, если поля title и value пусты, заменяем их на пустую строку
        foreach (var additionalField in userInput.additionalFields)
        {
            if (string.IsNullOrEmpty(additionalField.title) && string.IsNullOrEmpty(additionalField.value))
            {
                additionalField.title = ""; // Заменяем пустое поле title на пустую строку
                additionalField.value = ""; // Заменяем пустое поле value на пустую строку
            }
        }

        // Создаем новый объект для возврата, убирая пустые additionalFields
        var result = new PasswordDto
        {

            password = userInput.password,
            organization = userInput.organization,
            title = userInput.title,
            additionalFields = userInput.additionalFields
                .Where(field => !string.IsNullOrEmpty(field.title) || !string.IsNullOrEmpty(field.value))
                .ToList()
        };

        return Ok(result);
    }


    [HttpPut("UpdatePassword/{id}")]
    public IActionResult UpdatePassword(int id, [FromBody] PasswordDto userInput)
    {
        try
        {
            getUserId();
        }
        catch (Exception ex)
        {
            return StatusCode(401, ex.Message);
        }
        if (id == 0 || userInput == null)
        {
            return BadRequest("Invalid input data or user information");
        }

        try
        {
            string updatePasswordQuery = @"
            UPDATE dbo.Passwords 
            SET password = @password, organization =  @organization, title = @title 
            WHERE id = @id";

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand command = new SqlCommand(updatePasswordQuery, conn);
                command.Parameters.AddWithValue("@password", userInput.password ?? "");
                command.Parameters.AddWithValue("@organization", userInput.organization ?? "");
                command.Parameters.AddWithValue("@title", userInput.title ?? "");
                command.Parameters.AddWithValue("@id", id);
                int rowsAffected = command.ExecuteNonQuery();

                if (rowsAffected < 1)
                {
                    Console.WriteLine("Failed to update Passwords. No rows were affected.");
                    return StatusCode(500, "Failed to update Passwords");
                }

                Console.WriteLine("Passwords updated successfully");
                conn.Close();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception during password update: " + ex.Message);
            return StatusCode(500, "Exception during password update: " + ex.Message);
        }


        string deleteAdditionalFieldQuery = @"
        DELETE FROM dbo.AdditionalFields 
        WHERE passwordId = @id";

        using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
        {
            conn.Open();
            SqlCommand deleteCommand = new SqlCommand(deleteAdditionalFieldQuery, conn);
            deleteCommand.Parameters.AddWithValue("@id", id);

            int deletedRows = deleteCommand.ExecuteNonQuery();

            if (deletedRows < 0)
            {
                Console.WriteLine("Failed to delete existing AdditionalField entries");
                return StatusCode(500, "Failed to delete existing AdditionalField entries");
            }

            Console.WriteLine("All AdditionalField entries deleted successfully");
            conn.Close();
        }

        // Добавление новых данных в таблицу AdditionalFields с использованием текущего id
        foreach (var additionalField in userInput.additionalFields)
        {
            string insertAdditionalFieldQuery = @"
            INSERT INTO dbo.AdditionalFields (passwordId, title, [value])  
            VALUES (@passwordId, @title, @value)";

            using (SqlConnection conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                conn.Open();
                SqlCommand insertCommand = new SqlCommand(insertAdditionalFieldQuery, conn);
                insertCommand.Parameters.AddWithValue("@passwordId", id);
                insertCommand.Parameters.AddWithValue("@title", additionalField.title ?? "");
                insertCommand.Parameters.AddWithValue("@value", additionalField.value ?? "");

                int insertedRows = insertCommand.ExecuteNonQuery();

                if (insertedRows < 1)
                {
                    Console.WriteLine("Failed to add AdditionalField");
                    return StatusCode(500, "Failed to add AdditionalField");
                }

                Console.WriteLine("AdditionalField added successfully");
                conn.Close();
            }
        }

        return Ok();
    }



    [HttpDelete("DeletePassword/{id}")]
    public IActionResult DeletePassword(int id)
    {
        try
        {
            getUserId();
        }
        catch (Exception ex)
        {
            return StatusCode(401, ex.Message);
        }
        try
        {
            string countQuery = "SELECT COUNT(*) FROM dbo.AdditionalFields WHERE passwordId = " + id.ToString();
            var count = _dapper.LoadData<int>(countQuery).FirstOrDefault();

            if (count > 0)
            {
                string sqlAdditional = @"DELETE from dbo.AdditionalFields WHERE passwordId= " + id.ToString();

                if (!_dapper.ExecuteSQL(sqlAdditional))
                {
                    _logger.LogError($"Failed to delete user additional fields for ID: {id}");
                    return StatusCode(500, "Failed to delete user additional fields");
                }
            }

            string sqlPassword = @"DELETE from dbo.Passwords WHERE id= " + id.ToString();

            if (!_dapper.ExecuteSQL(sqlPassword))
            {
                _logger.LogError($"Failed to delete user password for ID: {id}");
                return StatusCode(500, "Failed to delete user password");
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