using ScholarshipPlatform.Courses.Dtos;

namespace ScholarshipPlatform.ScholarshipApplications.Dtos;

public record ApplicationResponseDto(
    int Id,
    DateOnly ApplicationDate,
    ApplicationStatus Status,
    string? UserName,
    int? UserId,
    string ScholarshipName,
    int ScholarshipId,
    List<CourseResponseDto> SelectedCourses,
    List<CourseResponseDto> EnrolledCourses
);