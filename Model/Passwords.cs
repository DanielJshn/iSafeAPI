using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;

namespace apitest;



public partial class Passwords
{
    private int userId;
    private int id;
    private string password;
    private string organization;
    private string title;
    private List<AdditionalField> additionalFields = new List<AdditionalField>();


    public Passwords()
    {
    }

    public Passwords(int userId, int id, string password, string organization, string title, List<AdditionalField> additionalFields)
    {
        this.userId = userId;
        this.id = id;
        this.password = password;
        this.organization = organization;
        this.title = title;
        this.additionalFields.AddRange(additionalFields);
    }

    public int UserId
    {
        get { return userId; }
        // No set accessor if you want it to be read-only
    }

    public int Id
    {
        get { return id; }
        set { id = value; }
    }

    public string Password
    {
        get { return password; }
        set { password = value; }
    }

    public string Organization
    {
        get { return organization; }
        set { organization = value; }
    }

    public string Title
    {
        get { return title; }
        set { title = value; }
    }
    public List<AdditionalField> AdditionalFields
    {
        get { 
            // User user = new User();
            return additionalFields;
             }
        set { additionalFields = value; }
    }
}

    // class Builder
    // {

    //     private Password password;
    //     public Password setPassword(string password)
    //     {
    //         password.Get = password
    //     ;
    //     return password
    //     }
    // }



// record User(int? id = null, string? name = null);