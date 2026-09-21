using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses.Dtos;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Scholarships.Dtos;
using ScholarshipPlatform.Users;
namespace ScholarshipPlatform.Scholarships;

public class ScholarshipService
{
    private readonly AppDbContext _db;

    public ScholarshipService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ScholarshipResponseDto?> CreateScholarship(CreateScholarshipDto dto)
    {
        var courses = await _db.Courses
            .Where(c => dto.CourseIds.Contains(c.Id))
            .ToListAsync();
        
        if(courses.Count != dto.CourseIds.Distinct().Count())
        return null;

        var scholarship = new Scholarship
        {
            Name = dto.Name,
            Country = dto.Country,
            Description = dto.Description,
            Deadline = dto.Deadline,
            Eligibility = dto.Eligibility,
            OfficialUrl = dto.OfficialUrl,
            RequiredDocuments = dto.RequiredDocuments,
            Courses = courses
        };

        _db.Scholarships.Add(scholarship);
        await _db.SaveChangesAsync();

        return new ScholarshipResponseDto(
            scholarship.Id,
            scholarship.Name,
            scholarship.Country,
            scholarship.Courses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList(),
            scholarship.Description,
            scholarship.Deadline,
            dto.Eligibility,
            dto.OfficialUrl,
            dto.RequiredDocuments
        );
    }

    public async Task<ScholarshipResponseDto?> GetScholarshipById(int id)
    {
        return await _db.Scholarships
        .Where(s => s.Id == id)
        .Select(s => new ScholarshipResponseDto(
            s.Id,
            s.Name,
            s.Country,
            s.Courses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList(),
            s.Description,
            s.Deadline,
            s.Eligibility,
            s.OfficialUrl,
            s.RequiredDocuments
        ))
        .FirstOrDefaultAsync();
    }

    public async Task<List<ScholarshipResponseDto>> GetScholarships()
    {
        return await _db.Scholarships
            .Select(s => new ScholarshipResponseDto(
                s.Id,
                s.Name,
                s.Country,
                s.Courses
                    .Select(c => new CourseResponseDto(c.Id, c.Name))
                    .ToList(),
                s.Description,
                s.Deadline,
                s.Eligibility,
                s.OfficialUrl,
                s.RequiredDocuments
            )).ToListAsync();
    }

    public async Task<bool?> UpdateScholarship(int id, PathScholarshipDto dto)
    {
        // 1. Busca a bolsa rastreando a relação atual
        var scholarship = await _db.Scholarships
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (scholarship is null)
            return null;

        // 2. Se CourseIds veio na requisição, substitui a coleção inteira
        if (dto.CourseIds is not null)
        {
            var newCourses = await _db.Courses
                .Where(c => dto.CourseIds.Contains(c.Id))
                .ToListAsync();

            if (newCourses.Count != dto.CourseIds.Distinct().Count())
            {
                return false; //Um ou mais cursos informados não existem.
            }

            scholarship.Courses = newCourses;
        }

        if(dto.Name is not null) scholarship.Name = dto.Name;
        if(dto.Country is not null) scholarship.Country = dto.Country;
        if(dto.Deadline is not null) scholarship.Deadline = dto.Deadline.Value;
        if(dto.Description is not null) scholarship.Description = dto.Description;
        if(dto.Eligibility is not null) scholarship.Eligibility = dto.Eligibility;
        if(dto.OfficialUrl is not null) scholarship.OfficialUrl = dto.OfficialUrl;
        if(dto.RequiredDocuments is not null) scholarship.RequiredDocuments = dto.RequiredDocuments; 

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool?> DeleteScholarship(int id)
    {
        var scholarship = await _db.Scholarships.FindAsync(id);

        if(scholarship is null) return false;

        var existApplication = await _db.ScholarshipApplications
            .AnyAsync(x => x.ScholarshipId == scholarship.Id);
            
        if(existApplication) return false;
        
        _db.Scholarships.Remove(scholarship);
        await _db.SaveChangesAsync();

        return true;
    }
}