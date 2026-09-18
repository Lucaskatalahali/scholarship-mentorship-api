using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Authentication;
using ScholarshipPlatform.Authentication.Dtos;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Email;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Users;

public class UserService
{
    private readonly UserManager<User> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private AppDbContext _db;

    public UserService(UserManager<User> userManager, ITokenService tokenService, AppDbContext db, IEmailService emailService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
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
            BirthDate = dto.BirthDate,
            Gpa = dto.Gpa,
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

        var confirmationLink = $"http://localhost:5274/users/confirm-email?userId={user.Id}&token={encodedToken}";

        await _emailService.SendEmailAsync(
            user.Email,
            "Email Confirmation",
            $"Click this link to confirm your email: {confirmationLink}"
        );

        Console.WriteLine(encodedToken);

        var userResponseDto = new UserResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.BirthDate,
            user.Gpa,
            user.AccountStatus
        );

        return ServiceResult<UserResponseDto>.Success(userResponseDto);
    }

    public async Task<bool> ConfirmEmail(int userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if(user is null) return false;

        // Decodificação do token que foi previamente codificada para transporte em URL
        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(token)
        );

        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

        return result.Succeeded;
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
            user.Gpa,
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
            u.Gpa,
            u.AccountStatus
            )
        ).ToListAsync();
    }

    public async Task<bool> UpdateUser(int id, PatchUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if(user is null) return false;

        if(dto.Name is not null) user.Name = dto.Name.Trim();
        if(dto.BirthDate is not null) user.BirthDate = dto.BirthDate;
        if(dto.Gpa is not null) user.Gpa = dto.Gpa;

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

    public async Task<string?> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        if(user is null) return null;

        if(
            !await _userManager.IsEmailConfirmedAsync(user) ||
            !await _userManager.CheckPasswordAsync(user, dto.Password)
        )
        {
            return null;
        }

        var roles = await _userManager.GetRolesAsync(user);

        return _tokenService.GenerateToken(user, roles);
    }

    public async Task<bool?> SuspendUser(string userEmail)
    {
        var user = await _userManager.FindByEmailAsync(userEmail);

        if(user == null) return null;

        if(user.AccountStatus != AccountStatus.Active)
            return false; //A conta já está suspensa por algum motivo

        // A conta está ativa e pode ser suspensa voluntariamente.
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