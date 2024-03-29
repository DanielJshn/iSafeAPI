using System.Collections;
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
    private readonly Datadapper _dapper;
    private readonly PasswordRepository passwordRepository;
    private readonly IConfiguration _config;

    public PasswordController(IConfiguration config)
    {
        _dapper = new Datadapper(config);
        _config = config;
        passwordRepository = new PasswordRepository(_dapper);
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
        List<Password> resultPasswords = passwordRepository.getAllPasswords(userId);

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
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok("User successfully deleted");
    }


    private ObjectResult? checkAuthToken()
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


    [NonAction]
    public int getUserId()
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