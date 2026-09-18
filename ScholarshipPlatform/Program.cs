using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Users;
using ScholarshipPlatform.Scholarships;
using ScholarshipPlatform.ScholarshipApplications;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ScholarshipPlatform.Payments;
using ScholarshipPlatform.Authentication;
using ScholarshipPlatform.Email;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.
    AddIdentityCore<User>()
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();

// Esta API vai usar o esquema Bearer para autenticação e JWT Bearer para processar os tokens.
builder.Services
    .AddAuthentication("Bearer")
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
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            )
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ScholarshipService>();
builder.Services.AddScoped<ScholarshipApplicationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>(); //AddTransient?
builder.Services.AddScoped<AuthenticationService>();


// Para serializar enums como strings, em vez de seus valores inteiros.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

//adicionando para testes com frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("TestClient", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});


var app = builder.Build();

//adicionando para test com frontend
app.UseCors("TestClient");

app.UseAuthentication();
app.UseAuthorization();


//Começar o app com as Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<int>>>();

    string[] roles = ["Mentorando", "Mentor", "Admin"];

    foreach(var role in roles)
    {
        if(!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
        }
    }
}

app.MapUserEndpoints();
app.MapScholarshipEndpoints();
app.MapScholarshipApplicationEndpoints();
app.MapPaymentEndpoints();
app.MapAuthenticationEndpoints()
;
app.Run();