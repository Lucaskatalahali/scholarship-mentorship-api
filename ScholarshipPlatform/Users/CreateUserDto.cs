namespace ScholarshipPlatform.Users;

public record CreateUserDto(
    string Name,
    string Email,
    string Password,
    decimal? Gpa,
    DateOnly? BirthDate
);