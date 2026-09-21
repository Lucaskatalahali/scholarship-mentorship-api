using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Courses.Dtos;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.ScholarshipApplications.Dtos;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.ScholarshipApplications;

public class ApplicationService
{
    private readonly AppDbContext _db;
    private readonly UserManager<User> _userManager;

    public ApplicationService(AppDbContext db, UserManager<User> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    public async Task<ServiceResult<ApplicationResponseDto>> CreateScholarshipApplication(int userId, CreateApplicationDto dto)
    {
        // Verificar se o usuário e a bolsa existem
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if(user is null)
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["UserId"] = ["User not found."]
                });
        }

        var scholarship = await _db.Scholarships
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == dto.ScholarshipId);

        if(scholarship is null)
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipId"] = ["Scholarship not found."]
                });
        } 

        //verificação antes de associar
        var validCourseIds = scholarship.Courses.Select(c => c.Id).ToHashSet();

        bool allValid = dto.SelectedCourseIds.All(validCourseIds.Contains);
        if (!allValid)
        {
             return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["CourseId"] = ["Um ou mais cursos selecionados não pertencem a esta bolsa."]
                });
        }

        var courses = await _db.Courses
            .Where(c => dto.SelectedCourseIds.Contains(c.Id))
            .ToListAsync();
        
        if(courses.Count != dto.SelectedCourseIds.Distinct().Count())
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["CourseId"] = ["Um ou mais cursos informados não existem."]
                });
        }

        //Verificar se já não existe um aplicativo desse usuário associado a essa bolsa
        var applicationExists = await _db.ScholarshipApplications
            .AnyAsync(s => s.UserId == userId && s.ScholarshipId == dto.ScholarshipId);

        if (applicationExists)
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipApplication"] = ["User has already applied for this scholarship."]
                });
        }

        //Criar a nova application
        var scholarshipApplication = new ScholarshipApplication
        {
            ApplicationDate = DateOnly.FromDateTime(DateTime.Today),
            UserId = userId,
            ScholarshipId = dto.ScholarshipId,
            SelectedCourses = courses,
        };

        _db.ScholarshipApplications.Add(scholarshipApplication);
        await _db.SaveChangesAsync();
        
        //Criar a resposta que irá para o cliente
        var ApplicationResponseDto = new ApplicationResponseDto(
            scholarshipApplication.Id,
            scholarshipApplication.ApplicationDate,
            scholarshipApplication.Status,
            user.Name,
            user.Id,
            scholarship.Name,
            scholarship.Id,
            scholarshipApplication.SelectedCourses
                .Select(c => new CourseResponseDto(c.Id,c.Name))
                .ToList(),
            scholarshipApplication.EnrolledCourses
                .Select(c => new CourseResponseDto(c.Id,c.Name))
                .ToList()
        );

        return ServiceResult<ApplicationResponseDto>.Success(ApplicationResponseDto);
    }
    public async Task<ApplicationResponseDto?> GetScholarshipApplicationById(int id)
    {
        var scholarshipApplicationDto = await _db.ScholarshipApplications
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new ApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User != null ? s.User.Name : null,
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId,
                s.SelectedCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList(),
                s.EnrolledCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList()
            ))
            .FirstOrDefaultAsync();

        return scholarshipApplicationDto;
    }

    public async Task<List<ApplicationResponseDto>> GetScholarshipApplications()
    {
        return await _db.ScholarshipApplications
            .Select(s => new ApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User != null ? s.User.Name : null,
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId,
                s.SelectedCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList(),
                s.EnrolledCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList()
            )).ToListAsync();
    }

    //O usuário obtem suas próprias aplicações
    public async Task<List<ApplicationResponseDto>> GetMyScholarshipApplications(int userId)
    {
        return await _db.ScholarshipApplications
            .Where(s => s.UserId == userId)
            .Select(s => new ApplicationResponseDto(
                s.Id,
                s.ApplicationDate,
                s.Status,
                s.User != null ? s.User.Name : null,
                s.UserId,
                s.Scholarship.Name,
                s.ScholarshipId,
                s.SelectedCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList(),
                s.EnrolledCourses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList()
            )).ToListAsync();
    }

    public async Task<bool> UpdateScholarshipApplication(int id, PatchScholarshipApplicationDto dto)
    {
        var scholarshipApplication = await _db.ScholarshipApplications.FindAsync(id);

        if(scholarshipApplication is null) return false;

        if(scholarshipApplication.Status == ApplicationStatus.Approved ||
            scholarshipApplication.Status == ApplicationStatus.Rejected
        )
        {
            return false;
        }

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