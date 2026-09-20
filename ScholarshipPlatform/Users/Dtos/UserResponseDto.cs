namespace ScholarshipPlatform.Users.Dtos;

public record UserResponseDto(
    int Id,
    string Name,
    string Email,
    DateOnly BirthDate,
    Address Address,
    EducationLevel EducationLevel,
    decimal Average,
    AccountStatus AccountStatus
);