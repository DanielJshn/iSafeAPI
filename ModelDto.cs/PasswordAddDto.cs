namespace apitest;



public partial class PasswordDto
{

    public string? organization { get; set; }
    public string? title { get; set; }
    public List<AdditionalFieldDto> additionalFields { get; set; }

    public PasswordDto()
    {
        additionalFields = new List<AdditionalFieldDto>();

    }
}