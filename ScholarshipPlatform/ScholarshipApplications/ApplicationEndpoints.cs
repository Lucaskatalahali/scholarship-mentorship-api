using System.Security.Claims;
using ScholarshipPlatform.ScholarshipApplications.Dtos;

namespace ScholarshipPlatform.ScholarshipApplications;

public static class ApplicationEndpoints
{
    public static RouteGroupBuilder MapApplicationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/applications");

        group.MapPost("/", CreateScholarshipApplication)
            .RequireAuthorization(policy => policy.RequireRole("Mentorando"));

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
        ClaimsPrincipal user,
        CreateApplicationDto dto, 
        ApplicationService scholarshipApplicationService
    )
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        return TypedResults.Unauthorized();

        var result = await scholarshipApplicationService.CreateScholarshipApplication(id, dto);

        if (!result.IsSuccess)
            return TypedResults.ValidationProblem(result.Errors!); //Depois tratar erros específicos

        return TypedResults.Created($"/scholarshipApplications/{result.Data!.Id}", result.Data);
    }

    private static async Task<IResult> GetScholarshipApplicationById(int id, ApplicationService scholarshipApplicationService)
    {
        if (id <= 0)
            return TypedResults.BadRequest(new { message = "O ID deve ser maior que 0." });

        var scholarshipApplicationDto = await scholarshipApplicationService.GetScholarshipApplicationById(id);

        return scholarshipApplicationDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(scholarshipApplicationDto);
    }

    //O usuário obtem suas próprias aplicações
    private static async Task<IResult> GetMyScholarshipApplications(
        ClaimsPrincipal user,
        ApplicationService scholarshipApplicationService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if(!int.TryParse(userId, out var id))
            return TypedResults.Unauthorized();

        var myScholarshipApplicationsDto = await scholarshipApplicationService.GetMyScholarshipApplications(id);

        return TypedResults.Ok(myScholarshipApplicationsDto);
    }

    private static async Task<IResult> GetScholarshipApplications(ApplicationService scholarshipApplicationService)
    {
        var scholarshipApplicationsDto = await scholarshipApplicationService.GetScholarshipApplications();

        return TypedResults.Ok(scholarshipApplicationsDto);
    }

    private static async Task<IResult> UpdateScholarshipApplication(
        int id, 
        PatchScholarshipApplicationDto dto, 
        ApplicationService scholarshipApplicationService)
    {
        if (id <= 0)
            return TypedResults.BadRequest(new { message = "O ID deve ser maior que 0." });
        
        var wasUpdated = await scholarshipApplicationService.UpdateScholarshipApplication(id, dto);

        return wasUpdated == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteScholarshipApplication(int id, ApplicationService scholarshipApplicationService)
    {
        if (id <= 0)
            return TypedResults.BadRequest(new { message = "O ID deve ser maior que 0." });
        
        var wasDeleted = await scholarshipApplicationService.DeleteScholarshipApplication(id);

        return wasDeleted == false
            ? TypedResults.NotFound()
            : TypedResults.NoContent();
    }
}