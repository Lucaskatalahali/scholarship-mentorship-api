using ScholarshipPlatform.Scholarships;
using ScholarshipPlatform.Users;

namespace ScholarshipPlatform.ScholarshipApplications;

public class ScholarshipApplication
{
    public int Id {get; set;}
    public DateOnly ApplicationDate {get; set;} //data registrada no nosso site
    public User User {get; set;} = null!; //o mentorando inscrito / relação mentorando - bolsa
    public int UserId {get; set;}
    public Scholarship Scholarship {get; set;} = null!;
    public int ScholarshipId {get; set;}
}