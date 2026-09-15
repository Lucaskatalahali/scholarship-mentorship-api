namespace ScholarshipPlatform.Users.Dtos;

public record PatchUserDto(
    string? Name,
    DateOnly? BirthDate,
    decimal? Gpa
);