
namespace apitest
{
    public interface IPasswordRepository
    {
        List<Password> GetAllPasswords(int userId);
        PasswordDto UpdatePassword(Guid id, PasswordDto userInput);
        PasswordDto PostPassword(int userId, PasswordDto passwordInput);
        void DeletePassword(Guid id);
    }
}