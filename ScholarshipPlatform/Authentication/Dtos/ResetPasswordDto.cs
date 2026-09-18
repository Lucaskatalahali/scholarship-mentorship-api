namespace ScholarshipPlatform.Authentication.Dtos;

public record ResetPasswordDto(
    int UserId,
    string Token,
    string NewPassword
);