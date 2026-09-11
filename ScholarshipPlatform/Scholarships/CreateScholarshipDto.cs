namespace ScholarshipPlatform.Scholarships;

public record CreateScholarshipDto(
    string Name,
    string? Description,
    DateOnly Deadline,
    string? Eligibility,
    string? HowToApply,
    string? OfficialUrl,
    string? RequiredDocuments
);