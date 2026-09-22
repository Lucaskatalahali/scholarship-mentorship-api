namespace ScholarshipPlatform.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseStoragePath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        // Salva fora da pasta pública web (não acessível diretamente via browser)
        _baseStoragePath = Path.Combine(env.ContentRootPath, "App_Data", "UserDocuments");

        if(!Directory.Exists(_baseStoragePath))
            Directory.CreateDirectory(_baseStoragePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder)
    {
        var targetDir = Path.Combine(_baseStoragePath, subFolder);
        Directory.CreateDirectory(targetDir);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDir, uniqueFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Retorna o caminho relativo (ex: "users/12/abc.pdf")
        return Path.Combine(subFolder, uniqueFileName).Replace("\\", "/");  
    }

    public Task<(Stream Stream, string ContentType)> GetFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_baseStoragePath, relativePath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("O ficheiro físico não foi encontrado no servidor.");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult((stream, "application/octet-stream"));
    }

    public Task DeleteFileAsync(string relativePath)
    {
        var fullPath = Path.Combine(_baseStoragePath, relativePath);
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}