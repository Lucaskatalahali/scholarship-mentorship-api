using System.ComponentModel.DataAnnotations;

namespace ScholarshipPlatform.Authentication.Dtos;

public record ForgotPasswordDto(
    [Required, EmailAddress] string Email
);