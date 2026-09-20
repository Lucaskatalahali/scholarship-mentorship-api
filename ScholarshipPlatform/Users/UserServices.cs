using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Authentication;
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
    private AppDbContext _db;

    public UserService(UserManager<User> userManager, ITokenService tokenService, AppDbContext db, IEmailService emailService)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
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

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        //Codificação adequada para transporte do token em uma URL

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

        if(user is null) 
            return ApprovalResult.NotFound;

        if(!await _userManager.IsEmailConfirmedAsync(user))
            return ApprovalResult.EmailNotConfirmed;

        if(user.AccountStatus == AccountStatus.Active)
            return ApprovalResult.AlreadyApproved;

        if(user.AccountStatus != AccountStatus.RegistrationPending)
        return ApprovalResult.InvalidStatus;
            

        user.AccountStatus = AccountStatus.Active;
        await _userManager.UpdateAsync(user);

        return ApprovalResult.Success;
    }

    public async Task<UserResponseDto?> GetUserById(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if(user is null) return null;
        
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
            )
        ).ToListAsync();
    }

    public async Task<bool> UpdateUser(int id, PatchUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if(user is null) return false;

        if(dto.Name is not null) user.Name = dto.Name.Trim();
        if(dto.BirthDate is not null) user.BirthDate = dto.BirthDate.Value;
        if(dto.Average is not null) user.Average = dto.Average.Value;
        if(dto.Adress.Country is not null) user.Address.Country = dto.Adress.Country;
        if(dto.Adress.Province is not null) user.Address.Province = dto.Adress.Province;
        if(dto.Adress.AddressLine is not null) user.Address.AddressLine = dto.Adress.AddressLine;
        if(dto.EducationLevel.HasValue) user.EducationLevel = dto.EducationLevel.Value;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded;
    }

    public async Task<bool> DeleteUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if(user is null) return false;
        
        var result = await _userManager.DeleteAsync(user);

        return result.Succeeded;
    }

    public async Task<bool?> SuspendUser(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if(user == null) return null;

        if(
            user.AccountStatus != AccountStatus.Active ||
            user.AccountStatus != AccountStatus.RegistrationPending
        )
        {
            return false; //A conta já está suspensa por algum motivo
        }
            
        // A conta está ativa e pode ser suspensa voluntariamente.
        //Contas pendentes não podem pedir suspensão
        user.AccountStatus = AccountStatus.SuspendedVoluntarily;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded; 
    }

    public async Task<bool?> ReactivateUser(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if(user == null) return null;

        if(user.AccountStatus == AccountStatus.Active)
            return false; //A conta já está activa

        // A conta está suspensa e pode ser reactivada sob uma justificativa prévia
        user.AccountStatus = AccountStatus.Active;

        var result = await _userManager.UpdateAsync(user);

        return result.Succeeded; 
    }
}