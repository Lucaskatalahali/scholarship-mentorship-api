namespace ScholarshipPlatform.ScholarshipApplications.Dtos;

public record CreateApplicationDto(
    int ScholarshipId,
    List<int> SelectedCourseIds
);