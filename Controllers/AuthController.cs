using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using api.Controllers;
using apitest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;

namespace apitest
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Datadapper _dapper;
        private readonly AuthHelp _authHelp;
        PasswordRepository passwordRepository;
        private readonly IConfiguration _config;
        AuthRepository authRepository;

        public AuthController(IConfiguration config)
        {
            _dapper = new Datadapper(config);
            _authHelp = new AuthHelp(config);
            _config = config;
            authRepository = new AuthRepository(_dapper, _authHelp, HttpContext);
            passwordRepository = new PasswordRepository(_dapper);
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            string token;
            try
            {
                authRepository.CheckUser(userForRegistration);
                token = authRepository.RegistrEndInsert(userForRegistration);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { Token = token });
        }


        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string newToken;
            try
            {
                string token = authRepository.CheckEmail(userForLogin);
                authRepository.CheckPassword(userForLogin);
                int userId = _authHelp.GetUserIdFromToken(token);
                if (userId == 0)
                {
                    return BadRequest("Can not find this user");
                }
                newToken = _authHelp.GenerateNewToken(userId);

                bool tokenUpdated = _authHelp.UpdateTokenValueInDatabase(userId, newToken);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { Token = newToken });
        }


        [HttpGet("RefreshToken")]
        public IActionResult RefreshToken()
        {
            string? refreshToken = HttpContext.Request.Headers["Authorization"];
            if (string.IsNullOrEmpty(refreshToken) || !refreshToken.StartsWith("Bearer "))
            {
                return BadRequest("Invalid or missing Bearer token in Authorization header");
            }

            refreshToken = refreshToken.Substring("Bearer ".Length);
            int userId = _authHelp.GetUserIdFromToken(refreshToken);
            if (userId == 0)
            {
                return BadRequest("Invalid Refresh Token");
            }

            string newToken = _authHelp.GenerateNewToken(userId);
            bool tokenUpdated = _authHelp.UpdateTokenValueInDatabase(userId, newToken);
            if (!tokenUpdated)
            {
                return StatusCode(500, "Failed to update token in the database");
            }

            return Ok(new { Token = newToken });
        }


        [HttpDelete("DeleteAllData")]
        public IActionResult DeleteAllData()
        {
            checkAuthToken();
            int userId = getUserId();
            try
            {
                passwordRepository.DeletePassword(userId);
                passwordRepository.DeleteUser(userId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Account deleted");
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
}



