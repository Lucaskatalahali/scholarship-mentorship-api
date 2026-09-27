using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using ScholarshipPlatform.Authentication.Dtos;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Email;
using ScholarshipPlatform.Users;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Authentication;

public class AuthenticationService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AuthenticationService> _logger;

    public AuthenticationService(
        UserManager<User> userManager, 
        ITokenService tokenService, 
        AppDbContext db, 
        IEmailService emailService,
        ILogger<AuthenticationService> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _emailService = emailService;
        _logger = logger;
    }
    
    public async Task<LoginResponseDto?> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            _logger.LogWarning("Tentativa de login com e-mail inexistente: {Email}", dto.Email);
            return null;
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogWarning("Login bloqueado: E-mail não confirmado para o usuário {UserId} ({Email})", user.Id, user.Email);
            return null;
        }

        if (user.AccountStatus == AccountStatus.RegistrationPending)
        {
            _logger.LogWarning("Login bloqueado: Conta com registro pendente de aprovação para o usuário {UserId}", user.Id);
            return null;
        }

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            _logger.LogWarning("Tentativa de login com senha incorreta para o usuário {UserId} ({Email})", user.Id, user.Email);
            return null;
        }

    var roles = await _userManager.GetRolesAsync(user);

        _logger.LogInformation("Usuário {UserId} ({Email}) autenticado com sucesso. Roles: {Roles}", 
            user.Id, user.Email, string.Join(", ", roles));

        var token = _tokenService.GenerateToken(user, roles);

        var userDto = new UserResponseDto(
            user.Id,
            user.Name,
            user.Email!,
            user.BirthDate,
            user.Address,
            user.EducationLevel,
            user.Average,
            user.AccountStatus
        );

        return new LoginResponseDto(token, userDto);
    }

    public async Task<bool> ConfirmEmail(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Confirmação de e-mail falhou: Usuário {UserId} não encontrado", userId);
            return false;
        }

        try
        {
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(token)
            );

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (result.Succeeded)
            {
                _logger.LogInformation("E-mail confirmado com sucesso para o usuário {UserId}", user.Id);
            }
            else
            {
                _logger.LogWarning("Token inválido ou expirado na confirmação de e-mail para o usuário {UserId}", user.Id);
            }

            return result.Succeeded;
        }
        catch (FormatException)
        {
            _logger.LogWarning("Token com formato corrompido na confirmação de e-mail para o usuário {UserId}", user.Id);
            return false;
        }
    }

    public async Task<bool> ResendEmailConfirmation(ResendEmailConfirmationDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null || user.EmailConfirmed) return false;

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
        
        _logger.LogInformation("Reenvio de link de confirmação de e-mail solicitado para o usuário {UserId}", user.Id);

        return true;
    }

    public async Task ForgotPassword(ForgotPasswordDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            _logger.LogInformation("Recuperação de senha solicitada para e-mail não cadastrado: {Email}", dto.Email);
            return;
        }

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

        _logger.LogInformation("Link de recuperação de senha gerado e enviado para o usuário {UserId}", user.Id);
    }

    public async Task<bool> ResetPassword(ResetPasswordDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());

        if (user is null)
        {
            _logger.LogWarning("Redefinição de senha falhou: Usuário {UserId} não encontrado", dto.UserId);
            return false;
        }

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

            if (result.Succeeded)
            {
                _logger.LogInformation("Senha redefinida com sucesso para o usuário {UserId}", user.Id);
            }
            else
            {
                _logger.LogWarning("Falha ao redefinir senha para o usuário {UserId}. Erros do Identity: {Errors}", 
                    user.Id, string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return result.Succeeded;
        }
        catch (FormatException)
        {
            _logger.LogWarning("Token com formato inválido na redefinição de senha para o usuário {UserId}", dto.UserId);
            return false;
        }
    }
}