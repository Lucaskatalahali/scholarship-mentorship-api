using ScholarshipPlatform.Courses.Dtos;

namespace ScholarshipPlatform.Scholarships.Dtos;

public record ScholarshipResponseDto(
    int Id,
    string Name,
    string Title,
    string Country,
    string Description,
    DateOnly Deadline,
    string OfficialUrl,
    string ScholarshipType,
    string? ImageUrl,
    List<string> Levels,
    List<string> Benefits,
    List<string> Eligibility,
    List<string> RequiredDocuments,
    List<CourseResponseDto> Courses
);