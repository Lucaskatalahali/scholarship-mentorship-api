using ScholarshipPlatform.Courses;

namespace ScholarshipPlatform.Scholarships;

public class Scholarship
{
    public int Id {get; set;}
    public required string Name {get; set;} // Ex.: BOLSA PARA BRASIL
    public required string Title {get; set;} // EX.: UNILAB
    public required string Country {get; set;}
    public required string Description {get; set;}
    public List<Course> Courses {get; set;} = [];
    public List<ScholarshipLevel> Levels {get; set;} = [];
    public List<string> Benefits {get; set;}= [];
    public List<string> RequiredDocuments {get; set;} = [];
    public List<string> Eligibility {get; set;} = [];
    public DateOnly Deadline {get; set;}
    public required string OfficialUrl {get; set;}
    public ScholarshipType ScholarshipType { get; set; } // Full or Partial
    public required string ImageUrl { get; set; } // Guarda o caminho retornado pelo storage
}