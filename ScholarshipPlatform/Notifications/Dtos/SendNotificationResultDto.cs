namespace ScholarshipPlatform.Notifications.Dtos;

public record SendNotificationResultDto(
    int TotalNotified,
    string Message
);