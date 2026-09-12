using Microsoft.AspNetCore.Identity;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Users;
using ScholarshipPlatform.Scholarships;
using ScholarshipPlatform.ScholarshipApplications;
using System.Text.Json.Serialization; //pra imprimir a string do enum, e não seu valor int.

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddValidatorsFromAssemblyContaining<Program>();

builder.Services.
    AddIdentityCore<User>()
    .AddRoles<IdentityRole<int>>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ScholarshipService>();
builder.Services.AddScoped<ScholarshipApplicationService>();


//Para imprimir o enum como string, e não pelo seu valor int.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();

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

app.MapUserEndpoins();
app.MapScholarshipEndpoints();
app.MapScholarshipApplicationEndpoints();

app.Run();