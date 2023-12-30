using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Identity.Client;
using Microsoft.VisualBasic;

namespace apitest;



public partial class UserData
{

  public int UserId { get; set; }

  public string Name { get; set; }

  public string UserSecret { get; set; }

  public UserData()
  {
    if (Name == null)
    {
      Name = "";
    }
    if (UserSecret == null)
    {
      UserSecret = "";
    }
  }
}
