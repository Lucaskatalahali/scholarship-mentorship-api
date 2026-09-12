namespace ScholarshipPlatform.ScholarshipApplications;

public record ScholarshipApplicationResponseDto(
    int Id,
    DateOnly ApplicationDate,
    ApplicationStatus Status,
    string UserName,
    int UserId,
    string ScholarshipName,
    int ScholarshipId
);