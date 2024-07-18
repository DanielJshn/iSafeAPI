using System.Collections;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest;
[Authorize]
[ApiController]
[Route("[controller]")]
public class PasswordController : ControllerBase
{
    private readonly IPasswordService _passwordService;
    private readonly IConfiguration _config;

    public PasswordController(IPasswordService passwordService, IConfiguration config)
    {
        _passwordService = passwordService;
        _config = config;
    }

    [HttpGet("GetPasswords")]
    public async Task<IActionResult> GetPasswords()
    {
        checkAuthToken();
        try
        {
            int userId = getUserId();
            List<Password> resultPasswords = await _passwordService.GetAllPasswordsAsync(userId);
            return Ok(resultPasswords);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddPassword")]
    public async Task<IActionResult> AddPassword([FromBody] PasswordDto userInput)
    {
        checkAuthToken();
        try
        {
            int userId = getUserId();
            PasswordDto createdPassword = await _passwordService.PostPasswordAsync(userId, userInput);
            return Ok(createdPassword);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdatePassword/{id}")]
    public async Task<IActionResult> UpdatePassword(Guid id, [FromBody] PasswordDto userInput)
    {
        checkAuthToken();
        try
        {
            PasswordDto updatedPassword = await _passwordService.UpdatePasswordAsync(id, userInput);
            return Ok(updatedPassword);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeletePassword/{id}")]
    public async Task<IActionResult> DeletePassword(Guid id)
    {
        checkAuthToken();
        try
        {
            await _passwordService.DeletePasswordAsync(id);
            return Ok("Password successfully deleted");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
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