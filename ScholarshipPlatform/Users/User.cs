using Microsoft.AspNetCore.Identity;

namespace ScholarshipPlatform.Users;

public class User : IdentityUser<int>
{
    public required string Name {get; set;}
    public decimal? Gpa {get; set;} //0-20
    public DateOnly? BirthDate {get; set;}
}