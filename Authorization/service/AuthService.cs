namespace apitest
{

    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public async Task CheckUserAsync(UserForRegistrationDto userForRegistration)
        {
            if (string.IsNullOrEmpty(userForRegistration.Email))
            {
                throw new Exception("Email is empty");
            }
            if (string.IsNullOrEmpty(userForRegistration.Password))
            {
                throw new Exception("Password is empty");
            }

            await _authRepository.CheckUserAsync(userForRegistration);
        }

        public async Task<string> RegistrEndInsertAsync(UserForRegistrationDto userForRegistration)
        {
            if (string.IsNullOrEmpty(userForRegistration.Email))
            {
                throw new Exception("Email is empty");
            }
            if (string.IsNullOrEmpty(userForRegistration.Password))
            {
                throw new Exception("Password is empty");
            }
            return await _authRepository.RegistrEndInsertAsync(userForRegistration);
        }

        public async Task<string> CheckEmailAsync(UserForLoginDto userForLogin)
        {
            if (string.IsNullOrEmpty(userForLogin.Email))
            {
                throw new Exception("Email is empty");
            }
            if (string.IsNullOrEmpty(userForLogin.Password))
            {
                throw new Exception("Password is empty");
            }
            return await _authRepository.CheckEmailAsync(userForLogin);
        }

        public async Task CheckPasswordAsync(UserForLoginDto userForLogin)
        {
            if (string.IsNullOrEmpty(userForLogin.Email))
            {
                throw new Exception("Email is empty");
            }
            if (string.IsNullOrEmpty(userForLogin.Password))
            {
                throw new Exception("Password is empty");
            }
            await _authRepository.CheckPasswordAsync(userForLogin);
        }

        public async Task<byte[]?> GetSaltForUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            return await _authRepository.GetSaltForUserIdAsync(userId);
        }

        public async Task<byte[]?> GetHashForUserIdAsync(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            return await _authRepository.GetHashForUserIdAsync(userId);
        }

        public async Task ChangeUserPasswordAsync(int userId, byte[] newPasswordHash)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            if (newPasswordHash == null || newPasswordHash.Length == 0)
            {
                throw new Exception("PasswordHash is null or empty");
            }
            await _authRepository.ChangeUserPasswordAsync(userId, newPasswordHash);
        }

        public async Task DeletePasswordDataAsync(List<Password> resultPasswords, int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            await _authRepository.DeletePasswordDataAsync(resultPasswords, userId);
        }

        public async Task DeleteUserAsync(int id)
        {
            if (id <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            await _authRepository.DeleteUserAsync(id);
        }
    }


}

