namespace ScholarshipPlatform.Storage;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(IFormFile file, string subFolder);
    Task<(Stream Stream, string ContentType)> GetFileAsync(string relativePath);
    Task DeleteFileAsync(string relativePath);
}