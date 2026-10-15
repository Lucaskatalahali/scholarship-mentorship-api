namespace ScholarshipPlatform.Users.Dtos;

public record UserResponseDto(
    int Id,
    string Name,
    string Email,
    string PhoneNumber,
    DateOnly BirthDate,
    Address? Address,
    EducationLevel? EducationLevel,
    decimal? Average,
    AccountStatus AccountStatus
);