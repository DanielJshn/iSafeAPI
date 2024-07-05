using System.Data;
using System.IdentityModel.Tokens.Jwt;

using apitest;
using Dapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace apitest;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    Datadapper _dapper;


    IConfiguration? _config;

    public TestController(IConfiguration config)
    {
        _dapper = new Datadapper(config);

    }

    [HttpGet("Connection")]
    public DateTime TestConnection()
    {
        return _dapper.LoadDataSingle<DateTime>("SELECT GETDATE()");
    }

    [HttpGet]
    public string Test()
    {
        return "iSafe API v2.1";
    }


}