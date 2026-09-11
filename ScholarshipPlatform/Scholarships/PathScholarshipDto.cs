namespace ScholarshipPlatform.Scholarships;

public record PathScholarshipDto(
    string? Name,
    string? Description,
    DateOnly? Deadline,
    string? Eligibility,
    string? HowToApply,
    string? OfficialUrl,
    string? RequiredDocuments
);
