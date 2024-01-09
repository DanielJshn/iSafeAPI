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
    PasswordRepository passwordRepository;
    private readonly ILogger<UserController> _logger;

    IConfiguration _config;

    public PasswordController(IConfiguration config, ILogger<UserController> logger)
    {
        _dapper = new Datadapper(config);
        _config = config;
        _logger = logger;
        passwordRepository = new PasswordRepository(_dapper, HttpContext, _config);
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return passwordRepository.TestConnection();
    }


    [HttpGet("GetPassword")]
    public IActionResult GetPasswords()
    {
        checkAuthToken();
        int userId = getUserId();

        if (userId == 0)
        {
            return BadRequest("Неверный или отсутствующий идентификатор пользователя");
        }

        List<Passwords> resultPasswords = passwordRepository.getAllPasswords(userId);

        if (resultPasswords == null || !resultPasswords.Any())
        {
            return NotFound("Пароли для указанного идентификатора пользователя не найдены");
        }

        return Ok(resultPasswords);
    }






    [HttpPost("AddPassword")]
    public IActionResult AddPassword([FromBody] PasswordDto userInput)
    {
        checkAuthToken();
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
        checkAuthToken();
        if (id == 0 || userInput == null)
        {
            return BadRequest("Invalid input data or user information");
        }

        try
        {
            passwordRepository.updatePassword(id, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }



    [HttpDelete("DeletePassword/{id}")]
    public IActionResult DeletePassword(int id)
    {
        checkAuthToken();
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


    private ObjectResult checkAuthToken()
    {
        try
        {
            getUserId();
        }
        catch (Exception ex)
        {
            return StatusCode(401, ex.Message);
        }
        return null;
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