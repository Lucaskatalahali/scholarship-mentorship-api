using ScholarshipPlatform.Authentication.Dtos;

namespace scholarshipPlatform.Authentication;
public enum LoginError
{
    None,
    InvalidCredentials,
    EmailNotConfirmed,
    RegistrationPending,
    AccountSuspended
}

public record LoginResult(
    LoginResponseDto? Response,
    LoginError Error
);
