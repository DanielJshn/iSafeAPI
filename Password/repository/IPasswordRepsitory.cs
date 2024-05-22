namespace apitest
{
    public interface IPasswordRepository
    {
        public List<Password> getAllPasswords(int userId);
        public PasswordDto UpdatePassword(Guid id, PasswordDto userInput);
        public PasswordDto PostPassword(int userId, PasswordDto passwordInput);
        public void DeletePassword(Guid id);
    }
}