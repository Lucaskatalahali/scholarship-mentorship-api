namespace ScholarshipPlatform.Users.Dtos;

public record UploadDocumentDto(
    DocumentType DocumentType,
    IFormFile File
);