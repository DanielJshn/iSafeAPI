using System.Text;
using apitest;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<KeyConfig>();
builder.Services.AddCors((options) =>
{
    options.AddPolicy("DevCors", (CorsBuilder) =>
    {
        CorsBuilder.WithOrigins("http://localhost:4200", "http://localhost:3000", "http://localhost:8000")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
    });
    options.AddPolicy("prodCors", (CorsBuilder) =>
    {
        CorsBuilder.WithOrigins("https://myProduct.com")
    .AllowAnyMethod()
    .AllowAnyHeader()
    .AllowCredentials();
    });

});


string? TokenKeyString = builder.Configuration.GetSection("AppSettings:TokenKey").Value;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(Options =>
{
    Options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TokenKeyString != null ? TokenKeyString : "")),
        ValidateIssuer = false,
        ValidateAudience = false

    };
});

builder.Services.AddScoped<Datadapper>();
builder.Services.AddScoped<DatadapperAsync>();
builder.Services.AddScoped<INoteRepository, NoteRepository>();
builder.Services.AddScoped<INotesService, NotesService>();
builder.Services.AddScoped<IPasswordRepository, PasswordRepository>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<AuthHelp>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");
}
else
{
    app.UseCors("prodCors");
    app.UseHttpsRedirection();
}

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
