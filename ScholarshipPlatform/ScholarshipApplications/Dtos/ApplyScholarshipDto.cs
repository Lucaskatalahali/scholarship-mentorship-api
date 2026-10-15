using ScholarshipPlatform.Scholarships;

namespace ScholarshipPlatform.ScholarshipApplications.Dtos;

public record ApplyScholarshipDto(
    int ScholarshipId,
    ScholarshipLevel SelectedLevel, // Agora o front manda o nível pretendido
    List<int> SelectedCourseIds
);