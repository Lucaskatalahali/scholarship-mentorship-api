namespace ScholarshipPlatform.Scholarships;

public static class ScholarshipEndpoins
{
    public static RouteGroupBuilder MapScholarshipEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/scholarships");

        group.MapPost("/", CreateScholarship);
        group.MapGet("/{id}", GetScholarship);
        group.MapGet("/", GetAllScholarships);
        group.MapPatch("/{id}", PatchScholarship);
        group.MapDelete("/{id}", DeleteScholarship);

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

    private static async Task<IResult> GetScholarship(int id, ScholarshipService scholarshipService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var scholarshipDto = await scholarshipService.GetScholarship(id);


        return scholarshipDto is null
            ? TypedResults.NotFound()
            :TypedResults.Ok(scholarshipDto);
    }

    private static async Task<IResult> GetAllScholarships(ScholarshipService scholarshipService)
    {
        var scholarshipsDto = await scholarshipService.GetAllScholarships();

        return TypedResults.Ok(scholarshipsDto);
    }

    private static async Task<IResult> PatchScholarship(
        int id, 
        PathScholarshipDto dto, 
        PathScholarshipDtoValidator validator, 
        ScholarshipService scholarshipService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid) return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var wasPatched = await scholarshipService.PatchScholarship(id, dto);

        return wasPatched
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