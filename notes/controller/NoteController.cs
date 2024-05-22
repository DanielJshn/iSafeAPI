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
    public NotesService _notesService;
    public CheckId _checkId;
    
    public NoteController(NotesService notesService,  CheckId checkId)
    {

        _notesService = notesService;
        _checkId = checkId;
        
    }

    [HttpPost("AddNote")]
    public IActionResult AddNote(NoteDto note)
    {
        _checkId.checkAuthToken();
        int userId = _checkId.getUserId();
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
        _checkId.checkAuthToken();
        int userId = _checkId.getUserId();
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
        _checkId.checkAuthToken();
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
        _checkId.checkAuthToken();
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
   
}
