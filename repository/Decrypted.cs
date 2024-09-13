
using System.Security.Cryptography;
using System.Text;

using apitest;


    public class KeyConfig
{
    private IConfiguration _config;

    public KeyConfig()
    {
        _config = new ConfigurationBuilder()
            .AddJsonFile("secureSettings.json", optional: true, reloadOnChange: true)
            .Build();
    }

    public string GetSecretKey()
    {
        return _config["AuthSecretKey"];
    }

    public string GetIV()
    {
        return _config["Vector"];
    }
}