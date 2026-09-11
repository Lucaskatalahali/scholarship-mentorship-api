using System.IO.Compression;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;

namespace ScholarshipPlatform.Users;

public class UserService
{
    private readonly UserManager<User> _userManager;

    public UserService(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ServiceResult<UserResponseDto>> CreateUser(CreateUserDto dto)
    {
        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email,
            UserName = dto.Email,
            BirthDate = dto.BirthDate,
            Gpa = dto.Gpa,
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            var errors = result.Errors
                .GroupBy(e => e.Code)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.Description).ToArray()
                );

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

            return ServiceResult<UserResponseDto>.Failure(errors);
        }

        var userResponseDto = new UserResponseDto(
            user.Id,
            user.Name,
            user.Email,
            user.BirthDate,
            user.Gpa
        );

        return ServiceResult<UserResponseDto>.Success(userResponseDto);
    }

    public async Task<UserResponseDto?> GetUser(int id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());

        if(user is null) return null;
        
        return new UserResponseDto(
            user.Id,
            user.Name,
            user.Email!,
            user.BirthDate,
            user.Gpa
        );
    }

    public async Task<List<UserResponseDto>> GetAllUsers()
    {
        return await _userManager.Users
        .Select(u => new UserResponseDto(
            u.Id,
            u.Name,
            u.Email!,
            u.BirthDate,
            u.Gpa
            )
        ).ToListAsync();
    }

    public async Task<bool> PatchUser(int id, PatchUserDto dto)
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
}