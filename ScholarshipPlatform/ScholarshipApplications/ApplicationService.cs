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
    private readonly ILogger<ApplicationService> _logger;

    public ApplicationService(
        AppDbContext db, 
        UserManager<User> userManager,
        ILogger<ApplicationService> logger)
    {
        _db = db;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ServiceResult<ApplicationResponseDto>> CreateScholarshipApplication(int userId, CreateApplicationDto dto)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());

        if (user is null)
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["UserId"] = ["User not found."]
                });
        }

        if (user.AccountStatus != AccountStatus.Active)
        {
            _logger.LogWarning("Tentativa de candidatura bloqueada: Usuário {UserId} não está ativo (Status: {Status})", 
                userId, user.AccountStatus);

            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["User"] = ["Não pode aplicar porque o usuário está suspenso."]
                });
        }

        var scholarship = await _db.Scholarships
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == dto.ScholarshipId);

        if (scholarship is null)
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipId"] = ["Scholarship not found."]
                });
        } 

        // Verificação antes de associar
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
        
        if (courses.Count != dto.SelectedCourseIds.Distinct().Count())
        {
            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["CourseId"] = ["Um ou mais cursos informados não existem."]
                });
        }

        // Verificar se já não existe candidatura desse usuário para esta bolsa
        var applicationExists = await _db.ScholarshipApplications
            .AnyAsync(s => s.UserId == userId && s.ScholarshipId == dto.ScholarshipId);

        if (applicationExists)
        {
            _logger.LogWarning("Candidatura duplicada rejeitada: Usuário {UserId} já possui inscrição para a bolsa {ScholarshipId}", 
                userId, dto.ScholarshipId);

            return ServiceResult<ApplicationResponseDto>.Failure(
                new Dictionary<string, string[]>
                {
                    ["ScholarshipApplication"] = ["User has already applied for this scholarship."]
                });
        }

        // Criar a nova aplicação
        var scholarshipApplication = new ScholarshipApplication
        {
            ApplicationDate = DateOnly.FromDateTime(DateTime.UtcNow),
            UserId = userId,
            ScholarshipId = dto.ScholarshipId,
            SelectedCourses = courses,
        };

        _db.ScholarshipApplications.Add(scholarshipApplication);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Candidatura {ApplicationId} criada com sucesso pelo usuário {UserId} para a bolsa {ScholarshipId}",
            scholarshipApplication.Id, userId, dto.ScholarshipId);
        
        var applicationResponseDto = new ApplicationResponseDto(
            scholarshipApplication.Id,
            scholarshipApplication.ApplicationDate,
            scholarshipApplication.Status,
            user.Name,
            user.Id,
            scholarship.Name,
            scholarship.Id,
            scholarshipApplication.SelectedCourses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList(),
            scholarshipApplication.EnrolledCourses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList()
        );

        return ServiceResult<ApplicationResponseDto>.Success(applicationResponseDto);
    }

    public async Task<ApplicationResponseDto?> GetScholarshipApplicationById(int id)
    {
        return await _db.ScholarshipApplications
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

        if (scholarshipApplication is null) return false;

        if (scholarshipApplication.Status == ApplicationStatus.Approved ||
            scholarshipApplication.Status == ApplicationStatus.Rejected)
        {
            _logger.LogWarning("Tentativa inválida de alterar status de candidatura {ApplicationId} que já está finalizada ({CurrentStatus})",
                id, scholarshipApplication.Status);
            return false;
        }

        var previousStatus = scholarshipApplication.Status;
        scholarshipApplication.Status = dto.Status;

        await _db.SaveChangesAsync();

        _logger.LogInformation("Status da candidatura {ApplicationId} atualizado de {PreviousStatus} para {NewStatus}",
            id, previousStatus, dto.Status);

        return true;
    }

    public async Task<bool> DeleteScholarshipApplication(int id)
    {
        var scholarshipApplication = await _db.ScholarshipApplications.FindAsync(id);

        if (scholarshipApplication is null) return false;

        _db.ScholarshipApplications.Remove(scholarshipApplication);
        await _db.SaveChangesAsync();

        return true;
    }
}