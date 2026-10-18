using ScholarshipPlatform.Scholarships;

namespace ScholarshipPlatform.SuccessStories.Dtos;

public record SuccessStoryResponseDto(
    int Id,
    string MenteeName,
    string PhotoUrl,
    string ScholarshipName,
    ScholarshipLevel AcademicLevel,
    string Country,
    int ApprovalYear,
    bool IsFeatured,
    string? Institution,
    string? Quote,
    string? Testimonial,
    string? VideoUrl
);