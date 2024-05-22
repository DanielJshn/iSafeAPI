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

    private readonly PasswordService _passwordService;
    private readonly CheckId _checkId;
    public PasswordController(PasswordService passwordService, CheckId checkId)
    {
        _passwordService = passwordService;
        _checkId = checkId;
    }


    [HttpGet("GetPassword")]
    public IActionResult GetPasswords()
    {
        _checkId.checkAuthToken();
        int userId = _checkId.getUserId();
        List<Password> resultPasswords = _passwordService.getAllPasswords(userId);
        return Ok(resultPasswords);
    }


    [HttpPost("AddPassword")]
    public IActionResult AddPassword([FromBody] PasswordDto userInput)
    {
        _checkId.checkAuthToken();
        int userId = _checkId.getUserId();
        PasswordDto CreatedPassword;
        try
        {
            CreatedPassword = _passwordService.PostPassword(userId, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok(CreatedPassword);
    }


    [HttpPut("UpdatePassword/{id}")]
    public IActionResult UpdatePassword(Guid id, [FromBody] PasswordDto userInput)
    {
        _checkId.checkAuthToken();
        PasswordDto UpdatePasswordData;
        try
        {
            UpdatePasswordData = _passwordService.UpdatePassword(id, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok(UpdatePasswordData);
    }


    [HttpDelete("DeletePassword/{id}")]
    public IActionResult DeletePassword(Guid id)
    {
        _checkId.checkAuthToken();
        try
        {
            _passwordService.DeletePassword(id);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok("User successfully deleted");
    }
}