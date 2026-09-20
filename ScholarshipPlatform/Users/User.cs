using Microsoft.AspNetCore.Identity;

namespace ScholarshipPlatform.Users;

public class User : IdentityUser<int>
{
    public required string Name {get; set;}
    public DateOnly BirthDate {get; set;}
    public Address Address {get; set;} = null!;
    public EducationLevel EducationLevel{get; set;}
    public decimal Average {get; set;} //0-20
    public AccountStatus AccountStatus {get; set;} = AccountStatus.RegistrationPending;
}