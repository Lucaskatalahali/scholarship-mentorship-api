using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Courses.Dtos;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Scholarships.Dtos;
using ScholarshipPlatform.Storage;

namespace ScholarshipPlatform.Scholarships;

public class ScholarshipService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storageService;
    private readonly ILogger<ScholarshipService> _logger;

    public ScholarshipService(AppDbContext db, IFileStorageService storageService, ILogger<ScholarshipService> logger)
    {
        _db = db;
        _logger = logger;
        _storageService = storageService;
    }

    public async Task<ScholarshipResponseDto?> CreateScholarship(CreateScholarshipDto dto)
    {
        // 1. Validação de existência dos cursos
        var courses = await _db.Courses
            .Where(c => dto.CourseIds.Contains(c.Id))
            .ToListAsync();

        if (courses.Count != dto.CourseIds.Distinct().Count())
        {
            _logger.LogWarning("Tentativa de criar bolsa '{Name}' falhou: Um ou mais cursos informados não existem", dto.Name);
            return null;
        }

        // 2. Salvar a imagem física via IFileStorageService
        string imagePath = string.Empty;
        if (dto.Image is not null && dto.Image.Length > 0)
        {
            // isPublic: true garante que o arquivo vai para wwwroot/uploads/scholarships
            imagePath = await _storageService.SaveFileAsync(dto.Image, "scholarships", isPublic: true);
        }

        // 3. Montar a entidade com os novos atributos
        var scholarship = new Scholarship
        {
            Name = dto.Name,
            Title = dto.Title,
            Country = dto.Country,
            Description = dto.Description!,
            Deadline = dto.Deadline,
            OfficialUrl = dto.OfficialUrl!,
            ScholarshipType = dto.ScholarshipType,
            ImageUrl = imagePath,
            
            Levels = string.IsNullOrWhiteSpace(dto.Levels)
                ? []
                : dto.Levels.Split(',', StringSplitOptions.TrimEntries)
                    .Select(level => Enum.Parse<ScholarshipLevel>(
                        level, ignoreCase: true))
                    .ToList(),

            Benefits = dto.Benefits,
            Eligibility = dto.Eligibility,
            RequiredDocuments = dto.RequiredDocuments,
            Courses = courses
        };

        _db.Scholarships.Add(scholarship);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Nova bolsa de estudos criada com sucesso: {ScholarshipId} - '{Name}' ({Country})",
            scholarship.Id, scholarship.Name, scholarship.Country);

        // 4. Retornar o novo ScholarshipResponseDto
        return new ScholarshipResponseDto(
            scholarship.Id,
            scholarship.Name,
            scholarship.Title,
            scholarship.Country,
            scholarship.Description ?? string.Empty,
            scholarship.Deadline,
            scholarship.OfficialUrl ?? string.Empty,
            scholarship.ScholarshipType.ToString(),
            scholarship.ImageUrl,
            scholarship.Levels.Select(l => l.ToString()).ToList(),
            scholarship.Benefits,
            scholarship.Eligibility,
            scholarship.RequiredDocuments,
            scholarship.Courses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList()
        );
    }

    
    public async Task<ScholarshipResponseDto?> GetScholarshipById(int id)
    {
        var scholarship = await _db.Scholarships
            .AsNoTracking()
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (scholarship is null)
            return null;

        return new ScholarshipResponseDto(
            scholarship.Id,
            scholarship.Name,
            scholarship.Title,
            scholarship.Country,
            scholarship.Description,
            scholarship.Deadline,
            scholarship.OfficialUrl,
            scholarship.ScholarshipType.ToString(),
            scholarship.ImageUrl,
            scholarship.Levels.Select(l => l.ToString()).ToList(),
            scholarship.Benefits,
            scholarship.Eligibility,
            scholarship.RequiredDocuments,
            scholarship.Courses
                .Select(c => new CourseResponseDto(c.Id, c.Name))
                .ToList()
        );
    }

    public async Task<List<ScholarshipResponseDto>> GetScholarships()
    {
        var scholarships = await _db.Scholarships
            .AsNoTracking()
            .Include(s => s.Courses)
            .ToListAsync();

        return scholarships.Select(s => new ScholarshipResponseDto(
            s.Id,
            s.Name,
            s.Title,
            s.Country,
            s.Description,
            s.Deadline,
            s.OfficialUrl,
            s.ScholarshipType.ToString(),
            s.ImageUrl,
            s.Levels.Select(l => l.ToString()).ToList(),
            s.Benefits,
            s.Eligibility,
            s.RequiredDocuments,
            s.Courses.Select(c => new CourseResponseDto(c.Id, c.Name)).ToList()
        )).ToList();
    }

    public async Task<bool?> UpdateScholarship(int id, PatchScholarshipDto dto)
    {
        var scholarship = await _db.Scholarships
            .Include(s => s.Courses)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (scholarship is null)
            return null;
        
        string? oldImagePath = null;
        string? newImagePath = null;

        // Atualização dos Cursos
        if (dto.CourseIds is not null)
        {
            var newCourses = await _db.Courses
                .Where(c => dto.CourseIds.Contains(c.Id))
                .ToListAsync();

            if (newCourses.Count != dto.CourseIds.Distinct().Count())
            {
                _logger.LogWarning("Falha ao atualizar bolsa {ScholarshipId}: Cursos informados não existem", id);
                return false;
            }

            scholarship.Courses = newCourses;
        }

        if (dto.Image is not null && dto.Image.Length > 0)
        {
            oldImagePath = scholarship.ImageUrl;

            newImagePath = await _storageService.SaveFileAsync(
                dto.Image, "scholarships", isPublic: true);

            scholarship.ImageUrl = newImagePath;
        }

        // 3. Atualização dos campos simples
        if (dto.Name is not null) scholarship.Name = dto.Name;
        if (dto.Title is not null) scholarship.Title = dto.Title;
        if (dto.Country is not null) scholarship.Country = dto.Country;
        if (dto.Deadline is not null) scholarship.Deadline = dto.Deadline.Value;
        if (dto.Description is not null) scholarship.Description = dto.Description;
        if (dto.OfficialUrl is not null) scholarship.OfficialUrl = dto.OfficialUrl;
        if (dto.ScholarshipType is not null) scholarship.ScholarshipType = dto.ScholarshipType.Value;

        // 4. Coleções de texto e enums
        if (dto.Levels is not null)
        {
            scholarship.Levels = dto.Levels
                .Split(',', StringSplitOptions.TrimEntries)
                .Select(level => Enum.Parse<ScholarshipLevel>(
                    level, ignoreCase: true))
                .ToList();
        }

        if (dto.Benefits is not null) scholarship.Benefits = dto.Benefits;
        if (dto.Eligibility is not null) scholarship.Eligibility = dto.Eligibility;
        if (dto.RequiredDocuments is not null) scholarship.RequiredDocuments = dto.RequiredDocuments;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            if (newImagePath is not null)
            {
                try
                {
                    await _storageService.DeleteFileAsync(
                        newImagePath, isPublic: true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Não foi possível excluir a nova imagem após falha na atualização da bolsa {ScholarshipId}",
                        id);
                }
            }

            throw;
        }

        // Só apagar a imagem antiga depois de guardar na BD.
        if (!string.IsNullOrWhiteSpace(oldImagePath))
        {
            try
            {
                await _storageService.DeleteFileAsync(
                    oldImagePath, isPublic: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Não foi possível excluir a imagem antiga {OldPath} da bolsa {ScholarshipId}",
                    oldImagePath, id);
            }
        }

        _logger.LogInformation(
            "Bolsa de estudos {ScholarshipId} ('{Name}') atualizada com sucesso",
            scholarship.Id, scholarship.Name);

        return true;
    }

    public async Task<bool?> DeleteScholarship(int id)
    {
        var scholarship = await _db.Scholarships.FindAsync(id);

        // Retorna null quando o registro não for encontrado (Endpoint devolve 404 Not Found)
        if (scholarship is null) 
            return null;

        var existApplication = await _db.ScholarshipApplications
            .AnyAsync(x => x.ScholarshipId == scholarship.Id);
            
        // Retorna false quando há conflito de integridade (Endpoint devolve 409 Conflict)
        if (existApplication)
        {
            _logger.LogWarning("Tentativa de exclusão rejeitada: A bolsa {ScholarshipId} possui candidaturas ativas vinculadas", id);
            return false;
        }

        // 1. Remove o registro do banco
        _db.Scholarships.Remove(scholarship);

        try
        {
            await _db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao apagar a bolsa {ScholarshipId}", id);
            throw;
        }

        // 2. Remove o arquivo físico da imagem (após confirmar o commit no banco)
        if (!string.IsNullOrWhiteSpace(scholarship.ImageUrl))
        {
            try
            {
                await _storageService.DeleteFileAsync(scholarship.ImageUrl, isPublic: true);
            }
            catch (Exception ex)
            {
                // Registra erro no log mas não quebra a resposta, pois o registro já foi removido do banco
                _logger.LogError(ex, "Falha ao excluir o arquivo físico da imagem da bolsa {ScholarshipId}: {Path}", id, scholarship.ImageUrl);
            }
        }

        _logger.LogInformation("Bolsa de estudos {ScholarshipId} excluída com sucesso", id);

        return true;
    }
}