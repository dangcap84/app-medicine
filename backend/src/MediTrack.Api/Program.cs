using Microsoft.EntityFrameworkCore;
using MediTrack.Infrastructure.Data;
using MediTrack.Application.Interfaces; // Add this using for IAuthService
using MediTrack.Infrastructure.Services; // Add this using for AuthService
using MediTrack.Infrastructure.BackgroundServices; // Add this using for NotificationGenerationService
using Microsoft.AspNetCore.Authentication.JwtBearer; // Add this using
using Microsoft.IdentityModel.Tokens; // Add this using
using System.Text; // Add this using
using System.Reflection; // Add this using for XML comments

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IMedicineService, MedicineService>(); // Register MedicineService
builder.Services.AddScoped<IScheduleService, ScheduleService>(); // Register ScheduleService
builder.Services.AddScoped<IUserProfileService, UserProfileService>(); // Register UserProfileService
builder.Services.AddScoped<INotificationService, NotificationService>(); // Register NotificationService
builder.Services.AddHostedService<NotificationGenerationService>(); // Register background service

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // using System.Reflection;
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
});

// Add Authentication and JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("Jwt");
    var key = Encoding.ASCII.GetBytes(jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key not configured"));

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key)
    };
});

builder.Services.AddAuthorization(); // Add Authorization services

// Add Controllers
builder.Services.AddControllers(); // Make sure controllers are added

var app = builder.Build();

// Configure the HTTP request pipeline.

// Add Development specific configurations if needed
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // Enable middleware to serve generated Swagger as a JSON endpoint.
    app.UseSwagger();
    // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
    // specifying the Swagger JSON endpoint.
    app.UseSwaggerUI(); // By default, it serves UI at /swagger
}

app.UseHttpsRedirection(); // Redirect HTTP to HTTPS

app.UseAuthentication(); // Add Authentication middleware
app.UseAuthorization(); // Add Authorization middleware

app.MapControllers(); // Map controller endpoints

app.Run();

// Remove the WeatherForecast record
