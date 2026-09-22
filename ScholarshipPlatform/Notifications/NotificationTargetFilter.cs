namespace ScholarshipPlatform.Notifications;

public record NotificationTargetFilter(
    bool AllUsers = false,
    int? ScholarshipId = null,
    int? MinAge = null,
    int? MaxAge = null,
    decimal? MinGpa = null,
    bool? OnlyUnpaidLastMonth = null,
    int? SpecificUserId = null
);