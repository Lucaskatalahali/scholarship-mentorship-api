
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
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
using ScholarshipPlatform.Courses;
using ScholarshipPlatform.Storage;
using ScholarshipPlatform.Notifications;
using ScholarshipPlatform.Common.Exceptions;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

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

// Injeção de dependência:
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ScholarshipService>();
builder.Services.AddScoped<ApplicationService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<AuthenticationService>();
builder.Services.AddScoped<CourseService>();
builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddScoped<NotificationService>();

//Registro dos serviços de ProblemDetails e ExceptionHandler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

app.UseExceptionHandler();

// Pipeline HTTP e Documentação,
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // Expõe a especificação em /openapi/v1.json
    app.MapScalarApiReference();// Monta a interface interativa em /scalar/v1
}

//adicionando para test com frontend
app.UseCors("TestClient");

app.UseAuthentication();
app.UseAuthorization();

// Começar o app com as Roles
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider
        .GetRequiredService<RoleManager<IdentityRole<int>>>();
    var logger = scope.ServiceProvider
        .GetRequiredService<ILogger<Program>>();

    string[] roles = ["Mentorando", "Mentor", "Admin"];

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole<int>(role));
            logger.LogInformation("Role de sistema criada: {Role}", role);
        }
    }
}

app.MapUserEndpoints();
app.MapScholarshipEndpoints();
app.MapApplicationEndpoints();
app.MapPaymentEndpoints();
app.MapAuthenticationEndpoints();
app.MapCourseEndpoints();
app.MapDocumentEndpoints();
app.MapNotificationEndpoints();
app.MapNewsletterEndpoints();

app.Run();

//Código para trabalhar com Scalar
internal sealed class BearerSecuritySchemeTransformer(
    Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider authenticationSchemeProvider)
    : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        var authenticationSchemes =
            await authenticationSchemeProvider.GetAllSchemesAsync();

        if (authenticationSchemes.Any(scheme => scheme.Name == "Bearer"))
        {
            var securitySchemes =
                new Dictionary<string, IOpenApiSecurityScheme>
                {
                    ["Bearer"] = new OpenApiSecurityScheme
                    {
                        Type = SecuritySchemeType.Http,
                        Scheme = "bearer",
                        In = ParameterLocation.Header,
                        BearerFormat = "JWT"
                    }
                };

            document.Components ??= new OpenApiComponents();

            document.Components.SecuritySchemes = securitySchemes;
        }
    }
}