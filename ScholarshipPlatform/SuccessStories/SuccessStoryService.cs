using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Storage;
using ScholarshipPlatform.SuccessStories.Dtos;

namespace ScholarshipPlatform.SuccessStories;

public class SuccessStoryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<SuccessStoryService> _logger;
    private readonly IFileStorageService _storageService;

    public SuccessStoryService(AppDbContext db, ILogger<SuccessStoryService> logger, IFileStorageService storageService)
    {
        _db = db;
        _logger = logger;
        _storageService = storageService;
    }

    public async Task<SuccessStoryResponseDto> CreateSuccessStory(CreateSuccessStoryDto dto)
    {
        // Salvar a imagem física via IFileStorageService
        string imagePath;
        // isPublic: true garante que o arquivo vai para wwwroot/uploads/scholarships
        imagePath = await _storageService.SaveFileAsync(dto.Image!, "SuccessStories", isPublic: true);

        // Montar a entidade com os novos atributos
        var successStory = new SuccessStory
        {
            MenteeName = dto.MenteeName,
            ImageUrl = imagePath,
            ScholarshipName = dto.ScholarshipName,
            AcademicLevel = dto.AcademicLevel,
            Country = dto.Country,
            ApprovalYear = dto.ApprovalYear,
            IsFeatured = dto.IsFeatured,
            Institution = dto.Institution,
            Quote = dto.Quote,
            Testimonial = dto.Testimonial,
            VideoUrl = dto.VideoUrl
        };

        _db.SuccessStories.Add(successStory);
        await _db.SaveChangesAsync();

        return new SuccessStoryResponseDto(
            successStory.Id,
            successStory.MenteeName,
            successStory.ImageUrl,
            successStory.ScholarshipName,
            successStory.AcademicLevel,
            successStory.Country,
            successStory.ApprovalYear,
            successStory.IsFeatured,
            successStory.Institution,
            successStory.Quote,
            successStory.Testimonial,
            successStory.VideoUrl
        );
    }

    public async Task<List<SuccessStoryResponseDto>> GetSuccessStories()
    {
        return await _db.SuccessStories
            .AsNoTracking()
            .Select(s => new SuccessStoryResponseDto(
                s.Id,
                s.MenteeName,
                s.ImageUrl,
                s.ScholarshipName,
                s.AcademicLevel,
                s.Country,
                s.ApprovalYear,
                s.IsFeatured,
                s.Institution,
                s.Quote,
                s.Testimonial,
                s.VideoUrl
            )).ToListAsync();
    }

    public async Task<List<SuccessStoryResponseDto>> GetFeaturedSuccessStories()
    {
        return await _db.SuccessStories
            .AsNoTracking()
            .Where(s => s.IsFeatured)
            .Select(s => new SuccessStoryResponseDto(
                s.Id,
                s.MenteeName,
                s.ImageUrl,
                s.ScholarshipName,
                s.AcademicLevel,
                s.Country,
                s.ApprovalYear,
                s.IsFeatured,
                s.Institution,
                s.Quote,
                s.Testimonial,
                s.VideoUrl
            )).ToListAsync();
    }

    public async Task<bool?> UpdateSuccessStory(int id, PatchSuccessStoryDto dto)
    {
        var successStory = await _db.SuccessStories.FindAsync(id);
        if (successStory == null)
        {
            return null;
        }

        string? oldImagePath = null;
        string? newImagePath = null;

        if (dto.Image is not null && dto.Image.Length > 0)
        {
            oldImagePath = successStory.ImageUrl;

            newImagePath = await _storageService.SaveFileAsync(
                dto.Image, "SuccessStories", isPublic: true);

            successStory.ImageUrl = newImagePath;
        }

        // Atualizar os campos se eles não forem nulos
        if (dto.MenteeName is not null) successStory.MenteeName = dto.MenteeName;
        if (dto.ScholarshipName is not null) successStory.ScholarshipName = dto.ScholarshipName;
        if (dto.AcademicLevel is not null) successStory.AcademicLevel = dto.AcademicLevel.Value;
        if (dto.Country is not null) successStory.Country = dto.Country;
        if (dto.ApprovalYear is not null) successStory.ApprovalYear = dto.ApprovalYear.Value;
        if (dto.IsFeatured is not null) successStory.IsFeatured = dto.IsFeatured.Value;
        if (dto.Institution is not null) successStory.Institution = dto.Institution;
        if (dto.Quote is not null) successStory.Quote = dto.Quote;
        if (dto.Testimonial is not null) successStory.Testimonial = dto.Testimonial;
        if (dto.VideoUrl is not null) successStory.VideoUrl = dto.VideoUrl;

        try
        {
            await _db.SaveChangesAsync();
        }
        catch
        {
            // A base de dados falhou: tenta remover a nova imagem
            // para evitar deixar um ficheiro órfão.
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
                        "Não foi possível excluir a nova imagem após falha na atualização {SuccessStoryId}",
                        id);
                }
            }

            throw;
        }

        // Só apaga a imagem antiga depois de a BD guardar com sucesso.
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
                    "Não foi possível excluir a imagem antiga da história {SuccessStoryId}",
                    id);
            }
        }

        _logger.LogInformation(
            "Sucesso na atualização da história de sucesso {SuccessStoryId} ('{Name}')",
            successStory.Id, successStory.MenteeName);

        return true;
    }

    public async Task<bool?> DeleteSuccessStory(int id)
    {
        var successStory = await _db.SuccessStories.FindAsync(id);
        if (successStory == null)
            return null;
       

        _db.SuccessStories.Remove(successStory);
        await _db.SaveChangesAsync();

        // Tenta apagar a imagem associada, mas não falha se não conseguir.
        if (!string.IsNullOrWhiteSpace(successStory.ImageUrl))
        {
            try
            {
                await _storageService.DeleteFileAsync(
                    successStory.ImageUrl, isPublic: true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Não foi possível excluir a imagem da história de sucesso {SuccessStoryId}",
                    id);
            }
        }

        _logger.LogInformation(
            "Sucesso na exclusão da história de sucesso {SuccessStoryId} ('{Name}')",
            successStory.Id, successStory.MenteeName);

        return true;
    }
}