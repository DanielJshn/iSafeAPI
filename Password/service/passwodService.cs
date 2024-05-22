using Microsoft.IdentityModel.Tokens;

namespace apitest
{
    public class PasswordService :IPasswordRepository
    {
        private readonly PasswordRepository _passwordRepository;
        public PasswordService(PasswordRepository passwordRepository)
        {
            _passwordRepository = passwordRepository;
        }

        public List<Password> getAllPasswords(int userId)
        {
            if (userId == 0)
            {
                throw new Exception("User not found");
            }

            return _passwordRepository.getAllPasswords(userId);
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
            if (userInput.title == null)
            {
                throw new Exception("Title is empty");
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
            if (passwordInput.title.IsNullOrEmpty())
            {
                throw new Exception("Title is empty");
            }
            if (passwordInput.password.IsNullOrEmpty())
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