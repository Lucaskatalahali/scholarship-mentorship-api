namespace ScholarshipPlatform.Users;

public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles);
}