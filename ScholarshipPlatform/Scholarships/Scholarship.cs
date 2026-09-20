using ScholarshipPlatform.Courses;

namespace ScholarshipPlatform.Scholarships;

public class Scholarship
{
    public int Id {get; set;}
    public required string Name {get; set;}
    public required string Country {get; set;}
    public List<Course> Courses {get; set;} = [];
    public string? Description {get; set;}
    public DateOnly Deadline {get; set;}
    public string? Eligibility {get; set;}
    public string? OfficialUrl {get; set;}
    public string? RequiredDocuments {get; set;}
}