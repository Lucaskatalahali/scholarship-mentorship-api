namespace ScholarshipPlatform.Scholarships.Dtos;

public class PatchScholarshipDto
{
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Country { get; set; }
    public string? Description { get; set; }
    public DateOnly? Deadline { get; set; }
    public string? OfficialUrl { get; set; }
    public ScholarshipType? ScholarshipType { get; set; }

    // Se o admin selecionar uma nova imagem, ela substitui a antiga
    public IFormFile? Image { get; set; }

    // Coleções opcionais: se forem enviadas, substituem as anteriores
    public List<ScholarshipLevel>? Levels { get; set; }
    public List<string>? Benefits { get; set; }
    public List<string>? Eligibility { get; set; }
    public List<string>? RequiredDocuments { get; set; }
    public List<int>? CourseIds { get; set; }
}