namespace apitest;

public partial class PasswordDto
{
    public Guid id { get; set; }
    public string password { get; set; }
    public string organization { get; set; }
    public string title { get; set; }
    public string? lastEdit { get; set; } = null;
    public List<AdditionalFieldDto> additionalFields { get; set; }

    public PasswordDto()
    {
        additionalFields = new List<AdditionalFieldDto>();
        password = "";
        organization = "";
        title = "";
    }
}