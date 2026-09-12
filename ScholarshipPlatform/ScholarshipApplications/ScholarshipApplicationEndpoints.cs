using Npgsql.Replication;

namespace ScholarshipPlatform.ScholarshipApplications;

public static class ScholarshipApplicationEndpoints
{
    public static RouteGroupBuilder MapScholarshipApplicationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/scholarshipApplications");

        group.MapPost("/", CreateScholarshipApplication);
        group.MapGet("/{id}", GetScholarshipApplication);
        group.MapGet("/", GetAllScholarshipApplications);
        group.MapPatch("/{id}", PatchScholarshipApplication);
        group.MapDelete("/{id}", DeleteScholarshipApplication);

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

    private static async Task<IResult> GetScholarshipApplication(int id, ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0)
            return TypedResults.BadRequest("ID must be greater than 0");

        var scholarshipApplicationDto = await scholarshipApplicationService.GetScholarshipApplication(id);

        return scholarshipApplicationDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(scholarshipApplicationDto);
    }

    private static async Task<IResult> GetAllScholarshipApplications(ScholarshipApplicationService scholarshipApplicationService)
    {
        var scholarshipApplicationsDto = await scholarshipApplicationService.GetAllScholarshipApplications();

        return TypedResults.Ok(scholarshipApplicationsDto);
    }

    private static async Task<IResult> PatchScholarshipApplication(
        int id, 
        PatchScholarshipApplicationDto dto, 
        ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest("ID must be greater than 0");
        
        var result = await scholarshipApplicationService.PatchScholarshipApplication(id, dto);

        return result == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteScholarshipApplication(int id, ScholarshipApplicationService scholarshipApplicationService)
    {
        if(id <= 0) 
            return TypedResults.BadRequest("ID must be greater than 0");
        
        var result = await scholarshipApplicationService.DeleteScholarshipApplication(id);

        return result == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }
}