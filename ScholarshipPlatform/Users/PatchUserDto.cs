namespace ScholarshipPlatform.Users;

public record PatchUserDto(
    string? Name,
    DateOnly? BirthDate,
    decimal? Gpa
);