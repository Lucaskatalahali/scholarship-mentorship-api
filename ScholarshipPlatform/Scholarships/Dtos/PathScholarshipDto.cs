namespace ScholarshipPlatform.Scholarships.Dtos;

public record PathScholarshipDto(
    string? Name,
    string? Description,
    DateOnly? Deadline,
    string? Eligibility,
    string? HowToApply,
    string? OfficialUrl,
    string? RequiredDocuments
);
