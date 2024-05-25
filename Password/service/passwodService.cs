using Microsoft.IdentityModel.Tokens;

namespace apitest
{
    public class PasswordService 
    {
        private readonly IPasswordRepository _passwordRepository;

        public PasswordService(IPasswordRepository passwordRepository)
        {
            _passwordRepository = passwordRepository;
        }

        public List<Password> GetAllPasswords(int userId)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }

            var passwords = _passwordRepository.GetAllPasswords(userId);

            if (passwords == null || !passwords.Any())
            {
                return new List<Password>();
            }

            return _passwordRepository.GetAllPasswords(userId);
        }

        public PasswordDto UpdatePassword(Guid id, PasswordDto userInput)
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

            return _passwordRepository.UpdatePassword(id, userInput);
        }

        public PasswordDto PostPassword(int userId, PasswordDto passwordInput)
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

            return _passwordRepository.PostPassword(userId, passwordInput);
        }

        public void DeletePassword(Guid id)
        {
            if (id == Guid.Empty)
            {
                throw new Exception("Invalid id");
            }

            _passwordRepository.DeletePassword(id);
        }
    }
}