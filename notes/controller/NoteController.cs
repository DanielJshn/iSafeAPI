using System.Data.Common;
using apitest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace apitest;

[Authorize]
[ApiController]
[Route("[controller]")]
public class NoteController : ControllerBase
{

    private readonly NotesService _notesService;
    private readonly IConfiguration _config;

    public NoteController(IConfiguration config, NotesService notesService)
    {
        _config = config;
        _notesService = notesService;
    }

    [HttpPost("AddNote")]
    public IActionResult AddNote(NoteDto note)
    {

        checkAuthToken();
        int userId = getUserId();
        NoteDto CreatedNote;
        try
        {
            CreatedNote = _notesService.AddNote(userId, note);
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
        List<NoteResponse> notes;
        try
        {
            notes = _notesService.GetNotes(userId);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok(notes);
    }

    [HttpPut("UpdateData/{id}")]
    public IActionResult UpdateData(Guid id, [FromBody] NoteDto userInput)
    {
        checkAuthToken();
        NoteDto updateData;
        try
        {
            updateData = _notesService.UpdateNote(id, userInput);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
        return Ok(updateData);
    }


    [HttpDelete("DeleteData/{id}")]
    public IActionResult DeleteData(Guid id)
    {
        checkAuthToken();
        try
        {
            _notesService.DeleteNote(id);
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
    protected virtual int getUserId()
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