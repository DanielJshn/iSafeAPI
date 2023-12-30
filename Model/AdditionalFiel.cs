

namespace apitest
{
    public partial class AdditionalField
    {
        public int passwordId { get; set; }
        public int id { get; set; }
        public string title_ { get; set; }
        public string value { get; set; }

        public AdditionalField()
        {

            title_ ??= "";
            value ??= "";
        }
    }
}