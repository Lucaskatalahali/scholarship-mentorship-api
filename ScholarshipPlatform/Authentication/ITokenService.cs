using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.Authentication;

public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles);
}