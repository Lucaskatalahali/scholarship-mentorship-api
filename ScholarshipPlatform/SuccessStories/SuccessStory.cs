using MimeKit.Encodings;
using ScholarshipPlatform.Scholarships;

namespace ScholarshipPlatform.SuccessStories;

public class SuccessStory
{
    public int Id {get; set;}
    public required string MenteeName {get; set;}
    public required string ImageUrl {get; set;}
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