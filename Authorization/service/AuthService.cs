namespace apitest
{

    public class AuthService
    {

        private readonly IAuthRepository _authRepository;

        public AuthService(IAuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

        public void CheckUser(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Email == null)
            {
                throw new Exception("Email is empty");
            }
            if (userForRegistration.Password == null)
            {
                throw new Exception("password is empty");
            }

            _authRepository.CheckUser(userForRegistration);
        }

        public string RegistrEndInsert(UserForRegistrationDto userForRegistration)
        {
            if (userForRegistration.Email == null)
            {
                throw new Exception("Email is empty");
            }
            if (userForRegistration.Password == null)
            {
                throw new Exception("password is empty");
            }
            return _authRepository.RegistrEndInsert(userForRegistration);
        }
        public string CheckEmail(UserForLoginDto userForLogin)
        {
            if (userForLogin.Email == null)
            {
                throw new Exception("Email is empty");
            }
            if (userForLogin.Password == null)
            {
                throw new Exception("password is empty");
            }
            return _authRepository.CheckEmail(userForLogin);
        }
        public void CheckPassword(UserForLoginDto userForLogin)
        {
            if (userForLogin.Email == null)
            {
                throw new Exception("Email is empty");
            }
            if (userForLogin.Password == null)
            {
                throw new Exception("password is empty");
            }
            _authRepository.CheckPassword(userForLogin);
        }
        public byte[]? GetSaltForUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            return _authRepository.GetSaltForUserId(userId);
        }
        public byte[]? GetHashForUserId(int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            return _authRepository.GetHashForUserId(userId);
        }
        public void ChangeUserPassword(int userId, byte[] newPasswordHash)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            if (newPasswordHash == null)
            {
                throw new Exception("PasswordHash is Null");
            }
            _authRepository.ChangeUserPassword(userId, newPasswordHash);
        }
        public void DeletePasswordData(List<Password> resultPasswords, int userId)
        {
            if (userId <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            _authRepository.DeletePasswordData(resultPasswords, userId);
        }
        public void DeleteUser(int id)
        {
            if (id <= 0)
            {
                throw new Exception("The correct user ID is not specified");
            }
            _authRepository.DeleteUser(id);
        }

    }
}
