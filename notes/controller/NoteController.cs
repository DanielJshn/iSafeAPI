using System.Data.Common;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest;
// [Authorize]
[ApiController]
[Route("[controller]")]
public class NoteController : ControllerBase
{
    private readonly NotesService _notesService;
    private readonly CheckId _checkId;
    private readonly ILogger<NoteController> _logger;

    public NoteController(NotesService notesService, CheckId checkId, ILogger<NoteController> logger)
    {
        _notesService = notesService;
        _checkId = checkId;
        _logger = logger;
    }

    [HttpPost("AddNote/{id}")]
    public IActionResult AddNote(NoteDto note, int id)
    {
        // _checkId.checkAuthToken();
        NoteDto createdNote;
        try
        {
            createdNote = _notesService.AddNote(id, note);
            return Ok(createdNote);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while adding note.");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("GetNote")]
    public IActionResult GetNote()
    {
        try
        {
            int userId = _checkId.ValidateAndGetUserId();
            List<NoteResponse> notes = _notesService.GetNotes(userId);
            return Ok(notes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting notes.");
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("UpdateData/{id}")]
    public IActionResult UpdateData(Guid id, [FromBody] NoteDto userInput)
    {
        try
        {
            _checkId.ValidateAndGetUserId(); 
            NoteDto updatedData = _notesService.UpdateNote(id, userInput);
            return Ok(updatedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating note.");
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("DeleteData/{id}")]
    public IActionResult DeleteData(Guid id)
    {
        try
        {
            _checkId.ValidateAndGetUserId(); 
            _notesService.DeleteNote(id);
            return Ok("Note successfully deleted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting note.");
            return BadRequest(ex.Message);
        }
    }
}