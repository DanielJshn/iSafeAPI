namespace apitest
{
    public interface IAuthRepository
    {
        public void CheckUser(UserForRegistrationDto userForRegistration);
        public string RegistrEndInsert(UserForRegistrationDto userForRegistration);
        public string CheckEmail(UserForLoginDto userForLogin);
        public void CheckPassword(UserForLoginDto userForLogin);
        public byte[]? GetSaltForUserId(int userId);
        public byte[]? GetHashForUserId(int userId);
        public void ChangeUserPassword(int userId, byte[] newPasswordHash);
        public void DeletePasswordData(List<Password> resultPasswords, int userId);
        public void DeleteUser(int id);
    }
}