namespace ScholarshipPlatform.Notifications.Dtos;

public record SendNotificationRequestDto(
    string Title,
    string Message,
    bool SendEmail,
    NotificationTargetFilter Filter
);