using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Users;

public static class DocumentEndpoints
{
    public static RouteGroupBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/documents");

        // O mentorando envia os seus documentos
        group.MapPost("/me", UploadMyDocument)
            .RequireAuthorization(policy => policy.RequireRole("Mentorando"))
            .DisableAntiforgery();

        // O mentorando lista os seus documentos
        group.MapGet("/me", GetMyDocuments)
            .RequireAuthorization(policy => policy.RequireRole("Mentorando"));

        // Admin e Mentor consultam os documentos de qualquer aluno
        group.MapGet("/users/{userId:int}", GetUserDocuments)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        // Download do arquivo (Admin, Mentor ou o próprio dono do documento)
        group.MapGet("/{id:int}/download", DownloadDocument)
            .RequireAuthorization();

        // Remover documento
        group.MapDelete("/{id:int}", DeleteDocument)
            .RequireAuthorization();

        return group;
    }

    private static async Task<IResult> UploadMyDocument(
        ClaimsPrincipal user,
        [FromForm] DocumentType documentType,
        [FromForm] IFormFile file,
        DocumentService documentService)
    {
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if(!int.TryParse(userIdStr, out var userId))
            return TypedResults.Unauthorized();

        var dto = new UploadDocumentDto(documentType, file);
        var result = await documentService.UploadDocumentAsync(userId, dto);

        if(!result.IsSuccess)
            return TypedResults.ValidationProblem(result.Errors!);

        return TypedResults.Created($"/documents/{result.Data!.UserId}", result.Data);
    }

    private static async Task<IResult> GetMyDocuments(
        ClaimsPrincipal user,
        DocumentService documentService)
    {
        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if(!int.TryParse(userIdStr, out var userId))
            return TypedResults.Unauthorized();

        var documents = await documentService.GetUserDocumentsAsync(userId);
        return TypedResults.Ok(documents);
    }

    private static async Task<IResult> GetUserDocuments(
        int userId,
        DocumentService documentService)
    {
        if(userId <= 0) 
            return TypedResults.BadRequest(new { message = "ID inválido." });

        var documents = await documentService.GetUserDocumentsAsync(userId);
        return TypedResults.Ok(documents);
    }

    private static async Task<IResult> DownloadDocument(
        int id,
        ClaimsPrincipal user,
        DocumentService documentService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest(new { message = "ID inválido." });

        var metadata = await documentService.GetDocumentMetadataAsync(id);
        if (metadata is null) 
            return TypedResults.NotFound(new { message = "Documento não encontrado." });

        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if(!int.TryParse(userIdStr, out var requestingUserId))
            return TypedResults.Unauthorized();

        var isAdminOrMentor = user.IsInRole("Admin") || user.IsInRole("Mentor");
        var isOwner = metadata.UserId == requestingUserId;

        if(!isAdminOrMentor && !isOwner)
            return TypedResults.Forbid();

        var downloadData = await documentService.GetDocumentForDownloadAsync(id);
        if(downloadData is null) return TypedResults.NotFound();

        return TypedResults.File(
            downloadData.Value.Stream,
            downloadData.Value.ContentType,
            downloadData.Value.FileName
        );
    }

    private static async Task<IResult> DeleteDocument(
        int id,
        ClaimsPrincipal user,
        DocumentService documentService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest(new { message = "ID inválido." });

        var metadata = await documentService.GetDocumentMetadataAsync(id);
        if(metadata is null) return TypedResults.NotFound();

        var userIdStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = int.TryParse(userIdStr, out var requestingUserId);

        var isAdmin = user.IsInRole("Admin");
        var isOwner = metadata.UserId == requestingUserId;

        if(!isAdmin && !isOwner)
            return TypedResults.Forbid();

        var wasDeleted = await documentService.DeleteDocumentAsync(id);
        return wasDeleted ? TypedResults.NoContent() : TypedResults.NotFound();
    }
}