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
         PasswordDto CreatedPassword;

          try
        {
            CreatedPassword = passwordRepository.PostPassword(userId, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(CreatedPassword);
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
            passwordRepository.UpdatePassword(id, userInput);
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
            passwordRepository.DeletePassword(id);
           
            return Ok("User successfully deleted");
        }
        catch(Exception ex)
        {
            return BadRequest(ex.Message);
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
        return null ;
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