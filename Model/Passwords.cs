using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;

namespace apitest;



public partial class Passwords
{
    public int UserId { get; set; }
    public int id { get; set; }
    public string? password { get; set; }
    public string? organization { get; set; }
    public string? title { get; set; }
    public List<AdditionalField> additionalFields { get; set; } // Обновлено здесь

    public Passwords()
    {
        additionalFields = new List<AdditionalField>(); // Инициализируем список при создании объекта Passwords
    }

}