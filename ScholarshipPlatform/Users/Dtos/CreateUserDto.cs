namespace ScholarshipPlatform.Users.Dtos;

public record CreateUserDto(
    string Name,
    string Email,
    string Password,
    DateOnly BirthDate,
    string PhoneNumber,
    Address Address,
    EducationLevel EducationLevel,
    decimal Gpa    
);