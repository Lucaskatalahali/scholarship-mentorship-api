namespace ScholarshipPlatform.Storage;

public interface IFileStorageService
{
    // isPublic define se vai para wwwroot (público) ou App_Data (privado)
    Task<string> SaveFileAsync(IFormFile file, string subFolder, bool isPublic = false);
    Task<(Stream Stream, string ContentType)> GetFileAsync(string relativePath, bool isPublic = false);
    Task DeleteFileAsync(string relativePath, bool isPublic = false);
}