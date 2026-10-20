namespace ScholarshipPlatform.Scholarships.Dtos;

//Como há manipulação de arquivo, usarei class em vez de record, mas record também funcionaria
public class CreateScholarshipDto
{
    public required string Name { get; set; }
    public required string Title { get; set; }
    public required string Country { get; set; }
    public string? Description { get; set; }
    public DateOnly Deadline { get; set; }
    public string? OfficialUrl { get; set; }
    public ScholarshipType ScholarshipType { get; set; }

    // Arquivo físico enviado pelo formulário (opcional ou obrigatório)
    public IFormFile? Image { get; set; }

    // Listas de texto (marcadores do frontend)
    public List<string> Benefits { get; set; } = [];
    public List<string> RequiredDocuments { get; set; } = [];
    public List<string> Eligibility { get; set; } = [];
    public string Levels { get; set; } = "";

    // Relacionamento com cursos existentes
    public List<int> CourseIds { get; set; } = [];
}