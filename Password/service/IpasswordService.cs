namespace apitest
{
    public interface IPasswordService
    {
        Task<List<Password>> GetAllPasswordsAsync(int userId);
        Task<PasswordDto> UpdatePasswordAsync(Guid id, PasswordDto userInput);
        Task<PasswordDto> PostPasswordAsync(int userId, PasswordDto passwordInput);
        Task DeletePasswordAsync(Guid id);
    }
}