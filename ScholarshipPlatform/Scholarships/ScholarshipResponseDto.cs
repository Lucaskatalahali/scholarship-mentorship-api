namespace ScholarshipPlatform.Scholarships;

public record ScholarshipResponseDto(
    int Id,
    string Name,
    string? Description,
    DateOnly Deadline,
    string? Eligibility,
    string? HowToApply,
    string? OfficialUrl,
    string? RequiredDocuments
);