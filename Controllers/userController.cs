using System.Data;
using System.IdentityModel.Tokens.Jwt;
using api.Controllers;
using apitest;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace apitest;
[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    Datadapper _dapper;
    

    IConfiguration _config;

    public UserController(IConfiguration config)
    {
        _dapper = new Datadapper(config);
        _config = config;
       
    }

    [HttpGet("TestConnection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }


    [HttpGet("GetUserData/{userId}")]
    public IActionResult GetUserData(int userId)
    {
        try {
            getUserId();
        } catch (Exception ex) {
            return StatusCode(401, ex.Message);
        }
        string sql = @"SELECT * FROM dbo.UserData where userId =" + userId.ToString();

        UserData userData = _dapper.LoadDataSingle<UserData>(sql);

        return Ok(userData);

    }

    [HttpPost("AddUserData")]
    public IActionResult AddUSerData(UserData userData)
    {
         try {
            getUserId();
        } catch (Exception ex) {
            return StatusCode(401, ex.Message);
        }
         int userId = getUserId();
         userData.UserId = userId;
        string sql = @"INSERT INTO dbo.UserData(
        UserId,
        Name,
        UserSecret
        )VALUES(
            '" + userId +
        "','" + userData.Name +
            "', '" + userData.UserSecret + "')";
        
        

        if (!_dapper.ExecuteSQL(sql))
        {
            throw new Exception("Failed to add UserData");
        }
        return Ok(userData);
    }

    [HttpDelete("DeleteUserData/{userId}")]
    public IActionResult DeleteUserData(int userId)
    {
         try {
            getUserId();
        } catch (Exception ex) {
            return StatusCode(401, ex.Message);
        }
        
        string sql = "DELETE FROM dbo.UserData WHERE UserId = @UserId";
        var parameters = new { UserId = userId };

        if (!_dapper.ExecuteSQL(sql, parameters))
        {
            return NoContent();
        }

        return Ok();
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


