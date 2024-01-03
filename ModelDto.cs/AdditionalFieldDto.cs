
namespace apitest
{
    public partial class AdditionalFieldDto
    {
        
        public string title_ { get; set; }
        public string value { get; set; }

        public AdditionalFieldDto()
        {

            title_ ??= "";
            value ??= "";
        }
    }
}