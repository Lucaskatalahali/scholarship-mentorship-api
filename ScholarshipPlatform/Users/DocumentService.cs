using Microsoft.EntityFrameworkCore;
using ScholarshipPlatform.Common;
using ScholarshipPlatform.Data;
using ScholarshipPlatform.Storage;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Users;

public class DocumentService
{
    private readonly AppDbContext _db;
    private readonly IFileStorageService _storageService;
    private readonly ILogger<DocumentService> _logger;

    // Regras de segurança de arquivo
    private static readonly string[] AllowedExtensions = [".pdf", ".png", ".jpg", ".jpeg"];
    private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

    public DocumentService(
        AppDbContext db, 
        IFileStorageService storageService,
        ILogger<DocumentService> logger)
    {
        _db = db;
        _storageService = storageService;
        _logger = logger;
    }

    public async Task<ServiceResult<UserDocumentResponseDto>> UploadDocumentAsync(int userId, UploadDocumentDto dto)
    {
        var file = dto.File;

        if (file is null || file.Length == 0)
        {
            return ServiceResult<UserDocumentResponseDto>.Failure(new Dictionary<string, string[]>
            {
                ["File"] = ["Nenhum ficheiro foi fornecido."]
            });
        }

        if (file.Length > MaxFileSizeInBytes)
        {
            _logger.LogWarning("Upload rejeitado para o usuário {UserId}: Tamanho do arquivo ({FileSize} bytes) excede o limite de 10 MB", 
                userId, file.Length);

            return ServiceResult<UserDocumentResponseDto>.Failure(new Dictionary<string, string[]>
            {
                ["File"] = ["O tamanho do documento excede o limite máximo permitido de 10 MB."]
            });
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
        {
            _logger.LogWarning("Upload rejeitado para o usuário {UserId}: Extensão não permitida '{Extension}'", 
                userId, extension);

            return ServiceResult<UserDocumentResponseDto>.Failure(new Dictionary<string, string[]>
            {
                ["File"] = ["Formato não suportado. Envie ficheiros PDF, PNG, JPG ou JPEG."]
            });
        }

        var relativeFolder = Path.Combine("users", userId.ToString());
        var storedPath = await _storageService.SaveFileAsync(file, relativeFolder);

        var document = new UserDocument
        {
            UserId = userId,
            DocumentType = dto.DocumentType,
            OriginalFileName = Path.GetFileName(file.FileName),
            StoredFileName = storedPath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length
        };

        _db.UserDocuments.Add(document);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Documento {DocumentId} ({DocumentType}) enviado com sucesso pelo usuário {UserId}. Tamanho: {FileSize} bytes",
            document.Id, document.DocumentType, userId, document.FileSizeBytes);

        var responseDto = new UserDocumentResponseDto(
            document.Id,
            document.UserId,
            document.DocumentType,
            document.OriginalFileName,
            document.FileSizeBytes,
            document.UploadedAt
        );

        return ServiceResult<UserDocumentResponseDto>.Success(responseDto);
    }

    public async Task<List<UserDocumentResponseDto>> GetUserDocumentsAsync(int userId)
    {
        return await _db.UserDocuments
            .AsNoTracking()
            .Where(d => d.UserId == userId)
            .Select(d => new UserDocumentResponseDto(
                d.Id,
                d.UserId,
                d.DocumentType,
                d.OriginalFileName,
                d.FileSizeBytes,
                d.UploadedAt
            ))
            .ToListAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)?> GetDocumentForDownloadAsync(int documentId)
    {
        var document = await _db.UserDocuments.FindAsync(documentId);
        if (document is null) return null;

        var (stream, _) = await _storageService.GetFileAsync(document.StoredFileName);
        return (stream, document.ContentType, document.OriginalFileName);
    }

    public async Task<UserDocument?> GetDocumentMetadataAsync(int documentId)
    {
        return await _db.UserDocuments.FindAsync(documentId);
    }

    public async Task<bool> DeleteDocumentAsync(int documentId)
    {
        var document = await _db.UserDocuments.FindAsync(documentId);
        if (document is null) return false;

        await _storageService.DeleteFileAsync(document.StoredFileName);
        _db.UserDocuments.Remove(document);
        await _db.SaveChangesAsync();

        _logger.LogInformation("Documento {DocumentId} do usuário {UserId} foi excluído com sucesso", 
            document.Id, document.UserId);

        return true;
    }
}