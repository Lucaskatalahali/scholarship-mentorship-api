using System.ComponentModel.DataAnnotations;

namespace ScholarshipPlatform.Authentication.Dtos;

public record ResendEmailConfirmationDto(
    [Required, EmailAddress] string Email
);