namespace ScholarshipPlatform.Authentication.Dtos;

public record LoginDto(
    string Email,
    string Password
);