namespace ScholarshipPlatform.Users.Dtos;

public record UserDocumentResponseDto(
    int id,
    int UserId,
    DocumentType DocumentType,
    string OriginalFileName,
    long FileSizeBytes,
    DateTime UploadedAt
);