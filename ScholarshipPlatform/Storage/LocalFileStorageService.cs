namespace ScholarshipPlatform.Storage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _privateBasePath;
    private readonly string _publicBasePath;

    public LocalFileStorageService(IWebHostEnvironment env)
    {
        // 1. Privado (fora da web, protegido)
        _privateBasePath = Path.Combine(env.ContentRootPath, "App_Data");
        
        // 2. Público (servido diretamente pelo Kestrel via UseStaticFiles)
        var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        _publicBasePath = Path.Combine(webRoot, "uploads");

        Directory.CreateDirectory(_privateBasePath);
        Directory.CreateDirectory(_publicBasePath);
    }

    public async Task<string> SaveFileAsync(IFormFile file, string subFolder, bool isPublic = false)
    {
        var basePath = isPublic ? _publicBasePath : _privateBasePath;
        var targetDir = Path.Combine(basePath, subFolder);
        Directory.CreateDirectory(targetDir);

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var uniqueFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(targetDir, uniqueFileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        // Se for público, retorna a URL relativa pronta para o front: "/uploads/scholarships/foto.png"
        // Se for privado, retorna o caminho interno: "users/1/doc.pdf"
        if (isPublic)
        {
            return $"/uploads/{subFolder}/{uniqueFileName}".Replace("//", "/");
        }

        return Path.Combine(subFolder, uniqueFileName).Replace("\\", "/");
    }

    public Task<(Stream Stream, string ContentType)> GetFileAsync(string relativePath, bool isPublic = false)
    {
        var basePath = isPublic ? _publicBasePath : _privateBasePath;
        
        // Limpa prefixos caso venha a URL pública completa
        var cleanPath = relativePath.TrimStart('/').Replace("uploads/", "");
        var fullPath = Path.Combine(basePath, cleanPath);

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("O ficheiro físico não foi encontrado no servidor.");

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Task.FromResult((stream, "application/octet-stream"));
    }

    public Task DeleteFileAsync(string relativePath, bool isPublic = false)
    {
        var basePath = isPublic ? _publicBasePath : _privateBasePath;
        var cleanPath = relativePath.TrimStart('/').Replace("uploads/", "");
        var fullPath = Path.Combine(basePath, cleanPath);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}