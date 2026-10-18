using ScholarshipPlatform.Scholarships;

namespace ScholarshipPlatform.SuccessStories.Dtos;

public class CreateSuccessStoryDto
{
    public required string MenteeName {get; set;}
    public IFormFile? Image {get; set;}
    public required string ScholarshipName {get; set;}
    public ScholarshipLevel AcademicLevel {get; set;}
    public required string Country {get; set;}
    public  int ApprovalYear {get; set;}
    public bool IsFeatured {get; set;} = false;
    public string? Institution {get; set;}
    public string? Quote {get; set;}
    public string? Testimonial {get; set;}
    public string? VideoUrl {get; set;}
}
