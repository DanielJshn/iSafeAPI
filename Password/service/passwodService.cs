using Microsoft.IdentityModel.Tokens;

namespace apitest
{
    public class PasswordService : IPasswordService
    {
        private readonly IPasswordRepository _passwordRepository;

        public PasswordService(IPasswordRepository passwordRepository)
        {
            _passwordRepository = passwordRepository;
        }

        public async Task<List<Password>> GetAllPasswordsAsync(int userId)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }

            var passwords = await _passwordRepository.GetAllPasswords(userId);

            if (passwords == null || !passwords.Any())
            {
                return new List<Password>();
            }

            return passwords;
        }

        public async Task<PasswordDto> UpdatePasswordAsync(Guid id, PasswordDto userInput)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid id");
            }

            if (userInput == null)
            {
                throw new ArgumentNullException(nameof(userInput));
            }

            if (string.IsNullOrEmpty(userInput.title))
            {
                throw new Exception("Title is empty");
            }

            if (string.IsNullOrEmpty(userInput.password))
            {
                throw new Exception("Password is empty");
            }

            return await _passwordRepository.UpdatePassword(id, userInput);
        }

        public async Task<PasswordDto> PostPasswordAsync(int userId, PasswordDto passwordInput)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }

            if (passwordInput == null || passwordInput.additionalFields == null)
            {
                throw new Exception("Invalid input data");
            }

            if (string.IsNullOrEmpty(passwordInput.title))
            {
                throw new Exception("Title is empty");
            }

            if (string.IsNullOrEmpty(passwordInput.password))
            {
                throw new Exception("Password is empty");
            }

            return await _passwordRepository.PostPassword(userId, passwordInput);
        }

        public async Task DeletePasswordAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid id");
            }

            await _passwordRepository.DeletePassword(id);
        }
    }
}