namespace ScholarshipPlatform.Users;

public record UserResponseDto(
    int Id,
    string Name,
    string Email,
    DateOnly? BirthDate,
    decimal? Gpa 
);