namespace ScholarshipPlatform.Users.Dtos;

public record UserResponseDto(
    int Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    decimal? Gpa,
    AccountStatus AccountStatus
);