
namespace apitest
{
    public partial class AdditionalFieldDto
    {
        public Guid id { get; set; }
        public string title { get; set; }
        public string value { get; set; }

        public AdditionalFieldDto()
        {

            title ??= "";
            value ??= "";
        }
    }
}