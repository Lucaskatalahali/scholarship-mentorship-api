namespace ScholarshipPlatform.Notifications;

public record NotificationTargetFilter(
    bool AllUsers = false,
    bool IncludeSubscribers = false, // Se true, dispara o e-mail também para inscritos da newsletter
    int? ScholarshipId = null,
    int? MinAge = null,
    int? MaxAge = null,
    decimal? MinGpa = null,
    bool? OnlyUnpaidLastMonth = null,
    int? SpecificUserId = null
);