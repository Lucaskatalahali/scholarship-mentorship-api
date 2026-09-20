namespace ScholarshipPlatform.Scholarships.Dtos;

public record PathScholarshipDto(
    string? Name,
    string? Country,
    string? Description,
    DateOnly? Deadline,
    string? Eligibility,
    string? OfficialUrl,
    string? RequiredDocuments,
    List<int>? CourseIds // serão os novos, antigos serão apagados
);