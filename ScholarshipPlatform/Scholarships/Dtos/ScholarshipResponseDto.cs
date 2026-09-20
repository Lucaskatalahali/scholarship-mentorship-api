using ScholarshipPlatform.Courses;

namespace ScholarshipPlatform.Scholarships.Dtos;

public record ScholarshipResponseDto(
    int Id,
    string Name,
    string Country,
    List<Course> Courses,
    string? Description,
    DateOnly Deadline,
    string? Eligibility,
    string? OfficialUrl,
    string? RequiredDocuments
);