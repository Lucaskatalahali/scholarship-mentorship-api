using System.ComponentModel.DataAnnotations;

namespace ScholarshipPlatform.Notifications.Dtos;

public record SubscribeNewsletterRequestDto(
    [Required, EmailAddress] string Email
);

public record NewsletterStatusResponseDto(
    string Message,
    bool IsActive
);