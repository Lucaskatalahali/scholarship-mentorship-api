namespace ScholarshipPlatform.Users.Dtos;

public record UserDocumentResponseDto(
    int Id,
    int UserId,
    DocumentType DocumentType,
    string OriginalFileName,
    long FileSizeBytes,
    DateTime UploadedAt
);