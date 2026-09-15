using System.Security.Claims;
using Npgsql.Replication;
using ScholarshipPlatform.ScholarshipApplications.Dtos;

namespace ScholarshipPlatform.ScholarshipApplications;

public static class ScholarshipApplicationEndpoints
{
    public static RouteGroupBuilder MapScholarshipApplicationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/scholarshipApplications");

        group.MapPost("/", CreateScholarshipApplication)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        group.MapGet("/{id:int}", GetScholarshipApplicationById)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor")); 

        //Apenas o Admin tem o poder de ver todas as aplicações do sistema.
        group.MapGet("/", GetScholarshipApplications)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        //Um usuário pode ver suas próprias aplicações
        group.MapGet("/me", GetMyScholarshipApplications)
            .RequireAuthorization();

        group.MapPatch("/{id:int}", UpdateScholarshipApplication)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        group.MapDelete("/{id:int}", DeleteScholarshipApplication)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }

    private static async Task<IResult> CreateScholarshipApplication(
        CreateScholarshipApplicationDto dto, 
        ScholarshipApplicationService scholarshipApplicationService
    )
    {
        if(dto.UserId <= 0 || dto.ScholarshipId <= 0) 
            return TypedResults.BadRequest("ID must be greater than 0");

        var result = await scholarshipApplicationService.CreateScholarshipApplication(dto);

        if (!result.IsSuccess)
            return TypedResults.ValidationProblem(result.Errors!); //Depois tratar erros específicos

        return TypedResults.Created($"/scholarshipApplications/{result.Data!.Id}", result.Data);
    }

    private static async Task<IResult> GetScholarshipApplicationById(int id, ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0)
            return TypedResults.BadRequest("ID must be greater than 0");

        var scholarshipApplicationDto = await scholarshipApplicationService.GetScholarshipApplicationById(id);

        return scholarshipApplicationDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(scholarshipApplicationDto);
    }

    //O usuário obtem suas próprias aplicações
    private static async Task<IResult> GetMyScholarshipApplications(
        ClaimsPrincipal user,
        ScholarshipApplicationService scholarshipApplicationService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if(!int.TryParse(userId, out var id))
            return TypedResults.Unauthorized();

        var myScholarshipApplicationsDto = await scholarshipApplicationService.GetMyScholarshipApplications(id);

        return TypedResults.Ok(myScholarshipApplicationsDto);
    }

    private static async Task<IResult> GetScholarshipApplications(ScholarshipApplicationService scholarshipApplicationService)
    {
        var scholarshipApplicationsDto = await scholarshipApplicationService.GetScholarshipApplications();

        return TypedResults.Ok(scholarshipApplicationsDto);
    }

    private static async Task<IResult> UpdateScholarshipApplication(
        int id, 
        PatchScholarshipApplicationDto dto, 
        ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest("ID must be greater than 0");
        
        var wasUpdated = await scholarshipApplicationService.UpdateScholarshipApplication(id, dto);

        return wasUpdated == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteScholarshipApplication(int id, ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest("ID must be greater than 0");
        
        var wasDeleted = await scholarshipApplicationService.DeleteScholarshipApplication(id);

        return wasDeleted == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }
}