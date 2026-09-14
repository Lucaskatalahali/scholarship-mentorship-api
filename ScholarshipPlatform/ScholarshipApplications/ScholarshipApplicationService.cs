using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.ScholarshipApplications;

public class ScholarshipApplicationService
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public ScholarshipApplicationService(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ServiceResult<ScholarshipApplicationResponseDto>> CreateScholarshipApplication(CreateScholarshipApplicationDto dto)
    {
        // Verificar se o usuário e a bolsa existem
        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());

        if(user is null)
        {
            return ServiceResult<ScholarshipApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["UserId"] = ["User not found."]
                });
        }

        var scholarship = await _db.Scholarships.FindAsync(dto.ScholarshipId);

        if(scholarship is null)
        {
            return ServiceResult<ScholarshipApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipId"] = ["Scholarship not found."]
                });
        } 

        //Verificar se já não existe um aplicativo desse usuário associado a essa bolsa
        var scholarshipApplicationExist = await _db.ScholarshipApplications
            .AnyAsync(s => s.UserId == dto.UserId && s.ScholarshipId == dto.ScholarshipId);

        if (scholarshipApplicationExist)
        {
            return ServiceResult<ScholarshipApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipApplication"] = ["User has already applied for this scholarship."]
                });
        }

        //Criar a nova application
        var scholarshipApplication = new ScholarshipApplication
        {
            ApplicationDate = DateOnly.FromDateTime(DateTime.Today),
            UserId = dto.UserId,
            ScholarshipId = dto.ScholarshipId,
        };

        _db.ScholarshipApplications.Add(scholarshipApplication);
        await _db.SaveChangesAsync();
        
        //Criar a resposta que irá para o cliente
        var scholarshipApplicationResponseDto = new ScholarshipApplicationResponseDto(
            scholarshipApplication.Id,
            scholarshipApplication.ApplicationDate,
            scholarshipApplication.Status,
            user.Name,
            user.Id,
            scholarship.Name,
            scholarship.Id
        );

        return ServiceResult<ScholarshipApplicationResponseDto>.Success(scholarshipApplicationResponseDto);
    }
    public async Task<ScholarshipApplicationResponseDto?> GetScholarshipApplicationById(int id)
    {
        var scholarshipApplicationDto = await _db.ScholarshipApplications
            .Where(s => s.Id == id)
            .Select(s => new ScholarshipApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User == null ? null : s.User.Name,//aqui ainda tem sinal amarelo
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId
            )).FirstOrDefaultAsync();

        return scholarshipApplicationDto;
    }

    public async Task<List<ScholarshipApplicationResponseDto>> GetScholarshipApplications()
    {
        return await _db.ScholarshipApplications
            .Select(s => new ScholarshipApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User == null ? null : s.User.Name,//aqui ainda tem sinal amarelo
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId
            )).ToListAsync();
    }

    //O usuário obtem suas próprias aplicações
    public async Task<List<ScholarshipApplicationResponseDto>> GetMyScholarshipApplications(int userId)
    {
        return await _db.ScholarshipApplications
            .Where(s => s.UserId == userId)
            .Select(s => new ScholarshipApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User == null ? null : s.User.Name,//aqui ainda tem sinal amarelo
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId
            )).ToListAsync();
    }

    public async Task<bool> UpdateScholarshipApplication(int id, PatchScholarshipApplicationDto dto)
    {
        var scholarshipApplication = await _db.ScholarshipApplications.FindAsync(id);

        if(scholarshipApplication is null) return false;

        scholarshipApplication.Status = dto.Status;

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteScholarshipApplication(int id)
    {
        var scholarshipApplication = await _db.ScholarshipApplications.FindAsync(id);

        if(scholarshipApplication is null) return false;

        _db.ScholarshipApplications.Remove(scholarshipApplication);
        await _db.SaveChangesAsync();

        return true;
    }
}