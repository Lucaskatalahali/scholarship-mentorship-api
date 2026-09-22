namespace ScholarshipPlatform.Users;

public enum DocumentType
{
    Passport = 0,
    IDCard = 1,
    Photo = 2,
    Transcript = 3,
    Diploma = 4,
    Resume = 5,
    RecommendationLetter = 6,
    MotivationLetter = 7,
    EnglishCertificate = 8,
    Other = 9
}

public class UserDocument
{
    public int Id {get; set;}
    public int UserId {get; set;}
    public User User {get; set;} = null!;

    public DocumentType DocumentType {get; set;}
    public required string OriginalFileName {get; set;}
    public required string StoredFileName {get; set;}
    public required string ContentType {get; set;}
    public long FileSizeBytes {get; set;}
    public DateTime UploadedAt {get; set;} = DateTime.UtcNow;
}