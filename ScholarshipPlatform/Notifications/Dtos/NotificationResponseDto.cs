namespace ScholarshipPlatform.Notifications.Dtos;

public record NotificationResponseDto(
    int Id,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt,
    DateTime? ReadAt
);