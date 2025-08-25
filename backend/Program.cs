using CARDB_EF.Data;
using CARDB_EF.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Http;

var tempConfig = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = Directory.GetCurrentDirectory(),
    WebRootPath = tempConfig["WebRoot:Path"]
});

builder.WebHost.UseUrls("http://localhost:2525");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDbContext<CarDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("DefaultConnection"), opt => {
        opt.UseOracleSQLCompatibility(OracleSQLCompatibility.DatabaseVersion21);
    }));

builder.Services.AddScoped<CarService>();
builder.Services.AddScoped<FavoriteService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<OfferService>();
builder.Services.AddScoped<CarImageService>();
builder.Services.AddScoped<LogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddSingleton<FileLoggerService>();

var app = builder.Build();

var appLifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
var fileLogger = app.Services.GetRequiredService<FileLoggerService>();

appLifetime.ApplicationStarted.Register(() => {
    fileLogger.LogAsync("APPLICATION START: The application has successfully started.");
});
appLifetime.ApplicationStopping.Register(() => {
    fileLogger.LogAsync("APPLICATION STOP: The application is shutting down.");
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.Use(async (context, next) => {
    context.Request.EnableBuffering();
    await next();
});

app.MapControllers();
app.Run();