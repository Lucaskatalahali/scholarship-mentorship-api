using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Email;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Users;

public enum ApprovalResult
{
    Success,
    NotFound,
    AlreadyApproved,
    EmailNotConfirmed,
    InvalidStatus
}

public class UserService
{
    private readonly UserManager<User> _userManager;
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;
    private readonly ILogger<UserService> _logger;

    public UserService(
        UserManager<User> userManager,
        AppDbContext db, 
        IEmailService emailService,
        ILogger<UserService> logger)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserResponseDto>> RegisterUser(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email,
            UserName = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            BirthDate = dto.BirthDate,
            
            Address = new Address
            {
                Country = dto.Address.Country,
                Province = dto.Address.Province,
                AddressLine = dto.Address.AddressLine    
            },

            EducationLevel = dto.EducationLevel,
            Average = dto.Gpa,
        };

        // Criar user e adicionar role devem acontecer ao mesmo tempo
        await using var transaction = await _db.Database.BeginTransactionAsync();

        try
        {
            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                _logger.LogWarning("Falha ao registrar novo usuário para o e-mail {Email}. Erros do Identity: {Errors}",
                    dto.Email, string.Join(", ", result.Errors.Select(e => e.Description)));

                var errors = result.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                await transaction.RollbackAsync();
                return ServiceResult<UserResponseDto>.Failure(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Mentorando");

            if (!roleResult.Succeeded)
            {
                _logger.LogWarning("Falha ao vincular role 'Mentorando' ao usuário {UserId} ({Email})", user.Id, user.Email);

                var errors = roleResult.Errors
                    .GroupBy(e => e.Code)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.Description).ToArray()
                    );

                await transaction.RollbackAsync();
                return ServiceResult<UserResponseDto>.Failure(errors);
            }

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        _logger.LogInformation("Novo usuário registrado com sucesso: {UserId} ({Email}) com role Mentorando", user.Id, user.Email);

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token)
        );

        var confirmationLink = $"http://localhost:5274/auth/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailAsync(
            user.Email,
            "Email Confirmation",
            $"Click this link to confirm your email: {confirmationLink}"
        );

        var userResponseDto = new UserResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.BirthDate,
            user.Address,
            user.EducationLevel,
            user.Average,
            user.AccountStatus
        );

        return ServiceResult<UserResponseDto>.Success(userResponseDto);
    }

    public async Task<ApprovalResult> ApproveRegistration(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) 
            return ApprovalResult.NotFound;

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogWarning("Aprovação rejeitada: Usuário {UserId} tentou ser aprovado sem antes confirmar o e-mail", id);
            return ApprovalResult.EmailNotConfirmed;
        }

        if (user.AccountStatus == AccountStatus.Active)
            return ApprovalResult.AlreadyApproved;

        if (user.AccountStatus != AccountStatus.RegistrationPending)
        {
            _logger.LogWarning("Aprovação rejeitada: Usuário {UserId} possui status incompatível para aprovação ({Status})", id, user.AccountStatus);
            return ApprovalResult.InvalidStatus;
        }

        user.AccountStatus = AccountStatus.Active;
        await _userManager.UpdateAsync(user);

        _logger.LogInformation("Registro de conta do usuário {UserId} ({Email}) aprovado com sucesso. Status alterado para Active", user.Id, user.Email);

        return ApprovalResult.Success;
    }

    public async Task<UserResponseDto?> GetUserById(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return null;
        
        return new UserResponseDto(
            user.Id,
            user.Name,
            user.Email!,
            user.BirthDate,
            user.Address,
            user.EducationLevel,
            user.Average,
            user.AccountStatus
        );
    }

    public async Task<List<UserResponseDto>> GetUsers()
    {
        return await _userManager.Users
            .Select(u => new UserResponseDto(
                u.Id,
                u.Name,
                u.Email!,
                u.BirthDate,
                u.Address,
                u.EducationLevel,
                u.Average,
                u.AccountStatus
            )).ToListAsync();
    }

    public async Task<bool> UpdateUser(int id, PatchUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return false;

        if (dto.Name is not null) user.Name = dto.Name.Trim();
        if (dto.BirthDate is not null) user.BirthDate = dto.BirthDate.Value;
        if (dto.Average is not null) user.Average = dto.Average.Value;
        if (dto.Adress.Country is not null) user.Address.Country = dto.Adress.Country;
        if (dto.Adress.Province is not null) user.Address.Province = dto.Adress.Province;
        if (dto.Adress.AddressLine is not null) user.Address.AddressLine = dto.Adress.AddressLine;
        if (dto.EducationLevel.HasValue) user.EducationLevel = dto.EducationLevel.Value;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if (user is null) return false;
        
        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuário {UserId} excluído do sistema", id);
        }

        return result.Succeeded;
    }

    public async Task<bool?> SuspendUser(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null) return null;

        if (user.AccountStatus != AccountStatus.Active)
            return false;
            
        user.AccountStatus = AccountStatus.SuspendedVoluntarily;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("Usuário {UserId} ({Email}) teve sua conta suspensa voluntariamente", user.Id, user.Email);
        }

        return result.Succeeded; 
    }

    public async Task<bool?> ReactivateUser(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if (user == null) return null;

        if (user.AccountStatus == AccountStatus.Active ||
            user.AccountStatus == AccountStatus.RegistrationPending)
        {
            return false;
        }

        user.AccountStatus = AccountStatus.Active;

        var result = await _userManager.UpdateAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation("Conta do usuário {UserId} ({Email}) reativada com sucesso. Status alterado para Active", user.Id, user.Email);
        }

        return result.Succeeded; 
    }
}