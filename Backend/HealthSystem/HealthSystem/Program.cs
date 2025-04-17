using HealthSystem.Data;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using MySql.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Bugsnag.AspNet.Core;
using Microsoft.Extensions.Logging;
using HealthSystem.Services;


var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddControllers().AddJsonOptions(options =>
{
	options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Configure BugSnag
builder.Services.AddBugsnag(configuration =>
{
    configuration.ApiKey = "cc8bdb23dae08cfcc5e4eb3429eced49"; 
});

// Retrieve the DB_PASSWORD from environment variables
var dbPassword = builder.Configuration["DB_PASSWORD"];

// Update the connection string with the password from the environment variable
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection").Replace("{DB_PASSWORD}", dbPassword);

// Configure DbContext with MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

// CORS Configuration
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowAngularApp", policy =>
	{
		policy.WithOrigins("http://localhost:4200") // The URL of your Angular app
			  .AllowAnyHeader()
			  .AllowAnyMethod();
	});
});

//Twilio - Register the Service
builder.Services.Configure<TwilioSettings>(builder.Configuration.GetSection("Twilio"));
builder.Services.AddTransient<ITwilioService, TwilioService>();

builder.Services.AddSwaggerGen();

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
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:SecretKey"]))
		};
	});
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

//else
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

// Enable CORS (Cross-Origin Requests)
app.UseCors("AllowAngularApp");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();

// Add BugSnag Middleware to handle unhandled exceptions
app.Use(async (context, next) =>
{
    try
    {
        await next.Invoke();
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred");

       
        throw;
    }
});
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();