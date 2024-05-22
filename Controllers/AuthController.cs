using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

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
        KeyConfig _keycon;
        private readonly CheckId _checkId;

        public AuthController(IConfiguration config, KeyConfig keycon, CheckId checkId)
        {
            _dapper = new Datadapper(config);
            _authHelp = new AuthHelp(config);
            _config = config;
            _keycon = keycon;
            _checkId = checkId;
            authRepository = new AuthRepository(_dapper, _authHelp, HttpContext);
            passwordRepository = new PasswordRepository(_dapper);
        }


        [AllowAnonymous]
        [HttpPost("Register")]
        public IActionResult Register(UserForRegistrationDto userForRegistration)
        {
            string token;


            string secretKey = _keycon.GetSecretKey();

            string decryptedEmail = DecryptStringAES(userForRegistration.Email, secretKey);
            string decryptedPassword = DecryptStringAES(userForRegistration.Password, secretKey);

            userForRegistration.Email = decryptedEmail;
            userForRegistration.Password = decryptedPassword;

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

                string secretKey = _keycon.GetSecretKey();

                string decryptedEmail = DecryptStringAES(userForLogin.Email, secretKey);
                string decryptedPassword = DecryptStringAES(userForLogin.Password, secretKey);

                userForLogin.Email = decryptedEmail;
                userForLogin.Password = decryptedPassword;

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
        public IActionResult DeletedAllData()
        {
            
            int userId = _checkId.ValidateAndGetUserId();
            try
            {
                List<Password> resultPasswords = passwordRepository.GetAllPasswords(userId);
                foreach (Password password in resultPasswords)
                {
                    string sql = "DELETE FROM AdditionalFields WHERE passwordId = @PasswordId";
                    _dapper.ExecuteSQL(sql, new { PasswordId = password.id });
                }
                string sqlPassword = "DELETE FROM Passwords WHERE UserId = @UserId";
                _dapper.ExecuteSQL(sqlPassword, new { UserId = userId });
                DeleteUser(userId);

            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Account deleted");
        }

        [HttpPut("ChangePassword")]
        public IActionResult ChangePassword(UserForChangePassword userForLogin)
        {
           
            int userId = _checkId.ValidateAndGetUserId();
            try
            {
                string secretKey = _keycon.GetSecretKey();
                string decryptedPassword = DecryptStringAES(userForLogin.Password, secretKey);
                string decryptedNewPassword = DecryptStringAES(userForLogin.NewPassword, secretKey);
                userForLogin.Password = decryptedPassword;
                userForLogin.NewPassword = decryptedNewPassword;

                byte[] passwordSalt = authRepository.GetSaltForUserId(userId);
                byte[] oldPasswordHash = authRepository.GetHashForUserId(userId);

                byte[] passwordConfirmationHash = _authHelp.GetPasswordHash(userForLogin.Password, passwordSalt);

                if (!passwordConfirmationHash.SequenceEqual(oldPasswordHash))
                {
                    throw new Exception("Current password is incorrect");
                }

                byte[] newPasswordHash = _authHelp.GetPasswordHash(userForLogin.NewPassword, passwordSalt);

                authRepository.ChangeUserPassword(userId, newPasswordHash);

                return Ok("Password successfully Changed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

       
        [NonAction]
        static string DecryptStringAES(string cipherText, string key)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.KeySize = 128;
                aesAlg.BlockSize = 128;
                aesAlg.Mode = CipherMode.ECB;
                aesAlg.Padding = PaddingMode.PKCS7;

                aesAlg.Key = Encoding.UTF8.GetBytes(key);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV), CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
        public void DeleteUser(int id)
        {
            string sqlUser = "DELETE FROM dbo.Tokens WHERE UserId = @id";

            if (!_dapper.ExecuteSQL(sqlUser, new { id }))
            {
                throw new Exception("Failed to delete User");
            }
        }

    }
}



