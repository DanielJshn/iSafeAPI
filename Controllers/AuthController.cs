using System.Data;
using System.Security.Cryptography;
using api.Controllers;
using apitest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;

namespace apitest
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Datadapper _dapper;

        private readonly AuthHelp _authHelp;
        private readonly IMemoryCache? _cache;

        public AuthController(IConfiguration config)
        {
            _dapper = new Datadapper(config);

            _authHelp = new AuthHelp(config);


        }
        [AllowAnonymous]
        [HttpPost("Register")]
       public IActionResult Register(UserForRegistrationDto userForRegistration)
{

            string sqlCheckUserExists = "SELECT email FROM dbo.Tokens WHERE email = '" + userForRegistration.Email + "'";
            IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);

            if (existingUsers.Count() > 0)
            {
                throw new Exception("User with this email already exists!");
            }

            byte[] passwordSalt = new byte[128 / 8];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetNonZeroBytes(passwordSalt);
            }

            byte[] passwordHash = _authHelp.GetPasswordHash(userForRegistration.Password, passwordSalt);


            string token = _authHelp.CreateToken(userForRegistration.Email);



            string sqlAddAuth = @"
        INSERT INTO dbo.Tokens  ([Email], [PasswordHash], [PasswordSalt], [TokenValue]) 
        VALUES (@Email, @PasswordHash, @PasswordSalt, @TokenValue)";

            List<SqlParameter> sqlParameters = new List<SqlParameter>
    {
        new SqlParameter("@Email", SqlDbType.NVarChar) { Value = userForRegistration.Email },
        new SqlParameter("@PasswordHash", SqlDbType.VarBinary) { Value = passwordHash },
        new SqlParameter("@PasswordSalt", SqlDbType.VarBinary) { Value = passwordSalt },
        new SqlParameter("@TokenValue", SqlDbType.NVarChar) { Value = token }
    };
            _cache?.Remove("Key");
            if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
            {
                return Ok(new { Token = token });
            }
            else
            {
                throw new Exception("Failed to register user.");
            }
        }

        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForToken = @"SELECT [UserId], [TokenValue] FROM dbo.Tokens WHERE Email = '" +
                userForLogin.Email + "'";

            var token = _dapper.LoadData<Token>(sqlForToken).FirstOrDefault();

            if (token == null)
            {
                return StatusCode(401, "Invalid email or password!");
            }

            string sqlForHashAndSalt = @"SELECT [PasswordHash], [PasswordSalt] FROM dbo.Tokens WHERE Email = '" +
                userForLogin.Email + "'";

            var userForConfirmation = _dapper.LoadData<UserForLoginConfirmationDto>(sqlForHashAndSalt).FirstOrDefault();

            if (userForConfirmation == null)
            {
                return StatusCode(401, "Invalid email or password!");
            }

            byte[] passwordHash = _authHelp.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

            if (!passwordHash.SequenceEqual(userForConfirmation.PasswordHash))
            {
                return StatusCode(401, "Incorrect password!");
            }
            _cache?.Remove("Key");
            // Если учетные данные верны, возвращаем токен из базы данных
            return Ok(new { Token = token.TokenValue });
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

            _cache?.Remove("Key");
            return Ok(new { Token = newToken });
        }



    }
}



