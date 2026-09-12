using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Users;
namespace ScholarshipPlatform.Scholarships;

public class ScholarshipService
{
    private readonly AppDbContext _db;

    public ScholarshipService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<ScholarshipResponseDto> CreateScholarship(CreateScholarshipDto dto)
    {
        var scholarship = new Scholarship
        {
            Name = dto.Name,
            Description = dto.Description,
            Deadline = dto.Deadline,
            Eligibility = dto.Eligibility,
            HowToApply = dto.HowToApply,
            OfficialUrl = dto.OfficialUrl,
            RequiredDocuments = dto.RequiredDocuments
        };

        _db.Scholarships.Add(scholarship);
        await _db.SaveChangesAsync();

        return new ScholarshipResponseDto(
            scholarship.Id,
            scholarship.Name,
            scholarship.Description,
            scholarship.Deadline,
            dto.Eligibility,
            dto.HowToApply,
            dto.OfficialUrl,
            dto.RequiredDocuments
        );
    }

    public async Task<ScholarshipResponseDto?> GetScholarship(int id)
    {
        return await _db.Scholarships
        .Where(s => s.Id == id)
        .Select(s => new ScholarshipResponseDto(
            s.Id,
            s.Name,
            s.Description,
            s.Deadline,
            s.Eligibility,
            s.HowToApply,
            s.OfficialUrl,
            s.RequiredDocuments
        ))
        .FirstOrDefaultAsync();
    }

    public async Task<List<ScholarshipResponseDto>> GetAllScholarships()
    {
        return await _db.Scholarships
            .Select(s => new ScholarshipResponseDto(
                s.Id,
                s.Name,
                s.Description,
                s.Deadline,
                s.Eligibility,
                s.HowToApply,
                s.OfficialUrl,
                s.RequiredDocuments
            )).ToListAsync();
    }

    public async Task<bool> PatchScholarship(int id, PathScholarshipDto dto)
    {
        var scholarship = await _db.Scholarships.FindAsync(id);

        if(scholarship is null) return false;

        if(dto.Name is not null) scholarship.Name = dto.Name;
        if(dto.Deadline is not null) scholarship.Deadline = dto.Deadline.Value; //essa linha está dando erro
        if(dto.Description is not null) scholarship.Description = dto.Description;
        if(dto.Eligibility is not null) scholarship.Eligibility = dto.Eligibility;
        if(dto.HowToApply is not null) scholarship.HowToApply = dto.HowToApply;
        if(dto.OfficialUrl is not null) scholarship.OfficialUrl = dto.OfficialUrl;
        if(dto.RequiredDocuments is not null) scholarship.RequiredDocuments = dto.RequiredDocuments; 

        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeleteScholarship(int id)
    {
        var scholarship = await _db.Scholarships.FindAsync(id);

        if(scholarship is null) return false;

        _db.Scholarships.Remove(scholarship);

        await _db.SaveChangesAsync();

        return true;
    }
}