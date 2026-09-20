using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using ScholarshipPlatform.Authentication.Dtos;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Email;
using ScholarshipPlatform.Users;
namespace ScholarshipPlatform.Authentication;

public class AuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private AppDbContext _db;

    public AuthenticationService(UserManager<User> userManager, ITokenService tokenService, AppDbContext db, IEmailService emailService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _db = db;
        _emailService = emailService;
    }

    
    public async Task<string?> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return null;

        if(
            !await _userManager.IsEmailConfirmedAsync(user) ||
            user.AccountStatus == AccountStatus.RegistrationPending ||
            !await _userManager.CheckPasswordAsync(user, dto.Password)
        )
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return _tokenService.GenerateToken(user, roles);
    }

    public async Task<bool> ConfirmEmail(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if(user is null) return false;

        // Decodificação do token que foi previamente codificada para transporte em URL

        try
        {
            var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(token)
        );

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return result.Succeeded;
            
        }
        catch(FormatException)
        {
            return false;
        }
    }

    public async Task<bool> ResendEmailConfirmation(ResendEmailConfirmationDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null || user.EmailConfirmed) return false;

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var confirmationLink = $"http://localhost:5274/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Email Confirmation",
            $"Click this link to confirm your email: {confirmationLink}"
        );
        
        return true;
    }

    public async Task ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return;

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var resetLink = $"http://localhost:5274/auth/reset-password?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailAsync(
            user.Email!,
            "Password Reset Request",
            $"Click this link to reset your password: {resetLink}"
        );
    }

    public async Task<bool> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());

        if(user is null) return false;

         // Decodificação do token que foi previamente codificada para transporte em URL
        try
        {
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(dto.Token)
            );

            var result = await _userManager.ResetPasswordAsync(
                user, 
                decodedToken, 
                dto.NewPassword
            );

            return result.Succeeded;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}