using System.Data;
using System.Diagnostics.Tracing;
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
        
        private readonly AuthHelp _authHelp;
        private readonly IPasswordService _passwordService;
        private readonly IConfiguration _config;
        private readonly IAuthService _authService;
        KeyConfig _keycon;
        INotesService _notesService;


        public AuthController(IConfiguration config, KeyConfig keycon, IAuthService authService, AuthHelp authHelp, IPasswordService passwordService, INotesService notesService)
        {

            _authHelp = authHelp;
            _config = config;
            _keycon = keycon;
            _authService = authService;
            _passwordService = passwordService;
            _notesService = notesService;

        }
        [AllowAnonymous]
        [HttpPost("Encr")]
        public IActionResult Encrypted(UserForRegistrationDto userForRegistration)
        {

            string base64Key = _keycon.GetSecretKey();
            string base64IV = _keycon.GetIV();

            string encryptedEmail = EncryptStringAES(userForRegistration.Email, base64Key, base64IV);
            string encryptedPassword = EncryptStringAES(userForRegistration.Password, base64Key, base64IV);

            var result = new
            {
                EncryptedEmail = encryptedEmail,
                EncryptedPassword = encryptedPassword,
                Key = base64Key,
                IV = base64IV
            };

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("Register")]
        public async Task<IActionResult> Register(UserForRegistrationDto userForRegistration)
        {
            string token;
            string base64Key = _keycon.GetSecretKey();
            string base64IV = _keycon.GetIV();



            byte[] keyBytes = Convert.FromBase64String(base64Key);
            byte[] ivBytes = Convert.FromBase64String(base64IV);


            if (keyBytes.Length != 32)
                throw new ArgumentException("Invalid key length. Must be 32 bytes for AES-256.");
            if (ivBytes.Length != 16)
                throw new ArgumentException("Invalid IV length. Must be 16 bytes for AES.");


            string decryptedEmail = DecryptStringAES(userForRegistration.Email, base64Key, base64IV);
            string decryptedPassword = DecryptStringAES(userForRegistration.Password, base64Key, base64IV);

            userForRegistration.Email = decryptedEmail;
            userForRegistration.Password = decryptedPassword;

            try
            {
                await _authService.CheckUserAsync(userForRegistration);
                token = await _authService.RegistrEndInsertAsync(userForRegistration);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok(new { Token = token });
        }




        [AllowAnonymous]
        [HttpPost("Login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLogin)
        {
            string newToken;
            try
            {
                string secretKey = _keycon.GetSecretKey();
                string secretIv = _keycon.GetIV();

                string decryptedEmail = DecryptStringAES(userForLogin.Email, secretKey, secretIv);
                string decryptedPassword = DecryptStringAES(userForLogin.Password, secretKey, secretIv);

                userForLogin.Email = decryptedEmail;
                userForLogin.Password = decryptedPassword;

                string token = await _authService.CheckEmailAsync(userForLogin);
                await _authService.CheckPasswordAsync(userForLogin);
                int userId = _authHelp.GetUserIdFromToken(token);
                if (userId == 0)
                {
                    return BadRequest("Cannot find this user");
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
        public async Task<IActionResult> RefreshToken()
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
        public async Task<IActionResult> DeletedAllData()
        {
            int userId = getUserId();
            try
            {
                List<Password> resultPasswords = await _passwordService.GetAllPasswordsAsync(userId);
                await _authService.DeletePasswordDataAsync(resultPasswords, userId);
                await _notesService.DeleteAllNoteAsync(userId);
                await _authService.DeleteUserAsync(userId);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok("Account deleted");
        }
        

        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(UserForChangePassword userForLogin)
        {
            int userId = getUserId();
            try
            {

                string secretIv = _keycon.GetIV();

                string secretKey = _keycon.GetSecretKey();
                string decryptedPassword = DecryptStringAES(userForLogin.Password, secretKey, secretIv);
                string decryptedNewPassword = DecryptStringAES(userForLogin.NewPassword, secretKey, secretIv);
                userForLogin.Password = decryptedPassword;
                userForLogin.NewPassword = decryptedNewPassword;

                byte[] passwordSalt = await _authService.GetSaltForUserIdAsync(userId);
                byte[] oldPasswordHash = await _authService.GetHashForUserIdAsync(userId);

                byte[] passwordConfirmationHash = _authHelp.GetPasswordHash(userForLogin.Password, passwordSalt);

                if (!passwordConfirmationHash.SequenceEqual(oldPasswordHash))
                {
                    throw new Exception("Current password is incorrect");
                }

                byte[] newPasswordHash = _authHelp.GetPasswordHash(userForLogin.NewPassword, passwordSalt);

                await _authService.ChangeUserPasswordAsync(userId, newPasswordHash);

                return Ok("Password successfully changed");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }


        [NonAction]
        public static string DecryptStringAES(string cipherText, string key, string iv)
        {
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            byte[] keyBytes = Convert.FromBase64String(key);
            byte[] ivBytes = Convert.FromBase64String(iv);
            Console.WriteLine($"Before key{keyBytes}" + $"After{key}");
            Console.WriteLine($"Before iv {ivBytes}" + $"After{iv}");
            // Проверка длины ключа и IV
            if (keyBytes.Length != 32) // Для AES-256 требуется ключ длиной 32 байта
                throw new ArgumentException("Invalid key length. Must be 32 bytes for AES-256.");
            if (ivBytes.Length != 16) // IV для AES-CBC должен быть 16 байт
                throw new ArgumentException("Invalid IV length. Must be 16 bytes for AES-CBC.");

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256; // Для AES-256
                aes.BlockSize = 128; // Блокировка 128 бит (16 байт)
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
        }
        
        [NonAction]
        public static string EncryptStringAES(string plainText, string base64Key, string base64IV)
        {
            byte[] keyBytes = Convert.FromBase64String(base64Key);
            byte[] ivBytes = Convert.FromBase64String(base64IV);

            if (keyBytes.Length != 32) // Для AES-256 требуется 32 байта ключа
                throw new ArgumentException("Invalid key length. Must be 32 bytes for AES-256.");
            if (ivBytes.Length != 16) // IV должен быть 16 байт
                throw new ArgumentException("Invalid IV length. Must be 16 bytes for AES.");

            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256; // Для AES-256
                aes.BlockSize = 128; // Блокировка 128 бит (16 байт)
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;
                aes.Key = keyBytes;
                aes.IV = ivBytes;

                using (ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
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







