using System.Data.Common;
using api.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest;
[Authorize]
[ApiController]
[Route("[controller]")]
public class NoteController : ControllerBase
{
    private readonly Datadapper _dapper;
    public readonly IConfiguration _config;

    NoteRepository _noteRepository;
    public NoteController(IConfiguration config)
    {
        _dapper = new Datadapper(config);
        _config = config;

        _noteRepository = new NoteRepository(_dapper);
    }

    [HttpPost("AddNote")]
    public IActionResult AddNote(NoteDto note)
    {

        checkAuthToken();
        int userId = getUserId();
        NoteDto CreatedNote;
        try
        {
            CreatedNote = _noteRepository.PostNote(userId, note);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok(CreatedNote);
    }

    [HttpGet("GetNote")]
    public IActionResult GetNote()
    {
        checkAuthToken();
        int userId = getUserId();
        if (userId == 0)
        {
            return BadRequest("Неверный или отсутствующий идентификатор пользователя");
        }
        List<NoteResponse> notes = _noteRepository.getAllNotes(userId);
        return Ok(notes);
    }

    [HttpPut("UpdateData/{id}")]
    public IActionResult UpdateData(Guid id, [FromBody] NoteDto userInput)
    {
        checkAuthToken();
        if (id == null || userInput == null)
        {
            return BadRequest("Invalid input data or user information");
        }

        try
        {
            _noteRepository.UpdateNote(id, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }

        return Ok();
    }

    
    [HttpDelete("DeleteData/{id}")]
    public IActionResult DeleteData(Guid id)
    {
        checkAuthToken();
        try
        {
            _noteRepository.DeleteData(id);
        } catch (Exception ex)
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
