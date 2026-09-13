namespace ScholarshipPlatform.Scholarships;

public static class ScholarshipEndpoins
{
    public static RouteGroupBuilder MapScholarshipEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/scholarships");

        group.MapPost("/", CreateScholarship)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/{id:int}", GetScholarshipById)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        group.MapGet("/", GetScholarships);

        group.MapPatch("/{id:int}", UpdateScholarship)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapDelete("/{id:int}", DeleteScholarship)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }

    private static async Task<IResult> CreateScholarship(
        CreateScholarshipDto dto, 
        CreateScholarshipDtoValidator validator, 
        ScholarshipService scholarshipService
    )
    {
        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid) return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var responseDto = await scholarshipService.CreateScholarship(dto);

        return TypedResults.Created($"/scholarships/{responseDto.Id}", responseDto);
    }

    private static async Task<IResult> GetScholarshipById(int id, ScholarshipService scholarshipService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var scholarshipDto = await scholarshipService.GetScholarshipById(id);


        return scholarshipDto is null
            ? TypedResults.NotFound()
            :TypedResults.Ok(scholarshipDto);
    }

    private static async Task<IResult> GetScholarships(ScholarshipService scholarshipService)
    {
        var scholarshipsDto = await scholarshipService.GetScholarships();

        return TypedResults.Ok(scholarshipsDto);
    }

    private static async Task<IResult> UpdateScholarship(
        int id, 
        PathScholarshipDto dto, 
        PathScholarshipDtoValidator validator, 
        ScholarshipService scholarshipService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid) return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var wasUpdated = await scholarshipService.UpdateScholarship(id, dto);

        return wasUpdated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteScholarship(int id, ScholarshipService scholarshipService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var wasDeleted = await scholarshipService.DeleteScholarship(id);

        return wasDeleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}