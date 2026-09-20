namespace ScholarshipPlatform.Scholarships.Dtos;

public record CreateScholarshipDto(
    string Name,
    string Country,
    string? Description,
    DateOnly Deadline,
    string? Eligibility,
    string? OfficialUrl,
    string? RequiredDocuments,
    List<int> CourseIds // Identificadores dos cursos que o admin selecionou
);