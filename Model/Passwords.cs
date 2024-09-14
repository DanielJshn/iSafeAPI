using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;

namespace apitest;

public class Password
{
    public int? userId { get; set; }
    public Guid? id { get; set; }
    public string password { get; set; }
    public string organization { get; set; }
    public string title { get; set; }
    public string? lastEdit { get; set; } = null;
    public List<AdditionalField> additionalFields { get; set; }

    public Password()
    {
        additionalFields = new List<AdditionalField>();
        password = "";
        organization = "";
        title = "";
    }

}