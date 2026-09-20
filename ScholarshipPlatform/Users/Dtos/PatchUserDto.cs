namespace ScholarshipPlatform.Users.Dtos;

public record PatchUserDto(
    string? Name,
    DateOnly? BirthDate,
    decimal? Average,
    Address Adress,
    EducationLevel? EducationLevel
);