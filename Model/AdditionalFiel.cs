

namespace apitest
{
    public partial class AdditionalField
    {
        public Guid passwordId { get; set; }
        public Guid id { get; set; }
        public string title { get; set; }
        public string value { get; set; }

        public AdditionalField()
        {

            title ??= "";
            value ??= "";
        }
    }
}