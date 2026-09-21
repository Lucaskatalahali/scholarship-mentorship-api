using ScholarshipPlatform.Courses;
using ScholarshipPlatform.Courses.Dtos;

namespace ScholarshipPlatform.Scholarships.Dtos;

public record ScholarshipResponseDto(
    int Id,
    string Name,
    string Country,
    List<CourseResponseDto> Courses,
    string? Description,
    DateOnly Deadline,
    string? Eligibility,
    string? OfficialUrl,
    string? RequiredDocuments
);