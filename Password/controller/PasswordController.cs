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
    private readonly ILogger<PasswordController> _logger;

    public PasswordController(PasswordService passwordService, CheckId checkId, ILogger<PasswordController> logger)
    {
        _passwordService = passwordService;
        _checkId = checkId;
        _logger = logger;
    }

    [HttpGet("GetPasswords")]
    public IActionResult GetPasswords()
    {
        try
        {
            int userId = _checkId.ValidateAndGetUserId();
            List<Password> resultPasswords = _passwordService.GetAllPasswords(userId);
            return Ok(resultPasswords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting passwords.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("AddPassword")]
    public IActionResult AddPassword([FromBody] PasswordDto userInput)
    {
        try
        {
            int userId = _checkId.ValidateAndGetUserId();
            PasswordDto createdPassword = _passwordService.PostPassword(userId, userInput);
            return Ok(createdPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding password.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdatePassword/{id}")]
    public IActionResult UpdatePassword(Guid id, [FromBody] PasswordDto userInput)
    {
        try
        {
            _checkId.ValidateAndGetUserId(); // userId is not used, so removed the assignment
            PasswordDto updatedPassword = _passwordService.UpdatePassword(id, userInput);
            return Ok(updatedPassword);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating password.");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeletePassword/{id}")]
    public IActionResult DeletePassword(Guid id)
    {
        try
        {
            _checkId.ValidateAndGetUserId(); // userId is not used, so removed the assignment
            _passwordService.DeletePassword(id);
            return Ok("Password successfully deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting password.");
            return BadRequest(ex.Message);
        }
    }
}