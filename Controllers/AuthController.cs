using System.Data;
using System.Security.Cryptography;
using api.Controllers;
using apitest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace apitest
{
        [Authorize]
        [ApiController]
        [Route("[controller]")]
        public class AuthController : ControllerBase
        {
            private readonly Datadapper _dapper;
           
            private readonly AuthHelp _authHelp;
            
            public AuthController (IConfiguration config)
            {
                _dapper = new Datadapper(config);
               
                _authHelp = new AuthHelp(config);
         
            }
             [AllowAnonymous]
             [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
                string sqlCheckUserExists = "SELECT email FROM dbo.Auth WHERE email = '" +
                    userForRegistration.Email + "'";

                IEnumerable<string> existingUsers = _dapper.LoadData<string>(sqlCheckUserExists);
                if (existingUsers.Count() == 0)
                {
                    byte[] passwordSalt = new byte[128 / 8];
                    using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
                    {
                        rng.GetNonZeroBytes(passwordSalt);
                    }

                    byte[] passwordHash =_authHelp.GetPasswordHash(userForRegistration.Password, passwordSalt);

                    string sqlAddAuth = @"
                        INSERT INTO dbo.Auth  ([Email],
                        [PasswordHash],
                        [PasswordSalt]) VALUES ('" + userForRegistration.Email +
                        "', @PasswordHash, @PasswordSalt)";

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();

                    SqlParameter passwordSaltParameter = new SqlParameter("@PasswordSalt", SqlDbType.VarBinary);
                    passwordSaltParameter.Value = passwordSalt;

                    SqlParameter passwordHashParameter = new SqlParameter("@PasswordHash", SqlDbType.VarBinary);
                    passwordHashParameter.Value = passwordHash;

                    sqlParameters.Add(passwordSaltParameter);
                    sqlParameters.Add(passwordHashParameter);

                         
                  
 if (_dapper.ExecuteSqlWithParameters(sqlAddAuth, sqlParameters))
        {
            // Получить UserId по Email и сохранить токен
            int userId = _dapper.GetUserIdByEmail(userForRegistration.Email);
            string token = _authHelp.CreateToken(userId); // Создать токен
            Token newToken = new Token { UserId = userId, TokenValue = token };
            _dapper.SaveToken(newToken); // Сохранить токен в базу данных

            return Ok();
        }
                    throw new Exception("Failed to register user.");
                }
                throw new Exception("User with this email already exists!");
            }
     
        
        [AllowAnonymous]
        [HttpPost("Login")]
        public IActionResult Login(UserForLoginDto userForLogin)
        {
            string sqlForHashAndSalt = @"SELECT 
                [PasswordHash],
                [PasswordSalt] FROM dbo.Auth WHERE Email = '" +
                userForLogin.Email + "'";

            UserForLoginConfirmationDto userForConfirmation = _dapper
                .LoadDataSingle<UserForLoginConfirmationDto>(sqlForHashAndSalt);
                

            byte[] passwordHash = _authHelp.GetPasswordHash(userForLogin.Password, userForConfirmation.PasswordSalt);

             for (int index = 0; index < passwordHash.Length; index++)
            {
                if (passwordHash[index] != userForConfirmation.PasswordHash[index]){
                    return StatusCode(401, "Incorrect password!");
                }
            }
            
            string sqluserId=@"SELECT UserId FROM dbo.Auth WHERE Email =  '"
            + userForLogin.Email+ "'";

            int userId = _dapper.LoadDataSingle<int>(sqluserId);

            return Ok(new Dictionary<string , string>{
                {"token", _authHelp.CreateToken(userId)}
            });
        }
         [HttpGet ("RefreshToken")]

         public string RefreshToken()
         {
            string sqluserId=@"SELECT userId FROM dbo.UserData WHERE userId  = '"
            + User.FindFirst("userId")?.Value+ "'";
            int userId = _dapper.LoadDataSingle<int>(sqluserId);
            return _authHelp.CreateToken(userId);
         }
       



    }



}