using ScholarshipPlatform.Courses.Dtos;

namespace ScholarshipPlatform.Courses;

public static class CourseEndpoints
{
    public static RouteGroupBuilder MapCourseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/courses");

        group.MapPost("/", CreateCourse)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapGet("/", GetCourses)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPatch("/{id:int}", UpdateCourse);

        //group.MapDelete("/{id}", DeleteCourse)
        //  .RequireAuthorization(policy => policy.RequireRole("Admin"));


        return group;
    }

    private static async Task<IResult> CreateCourse(CreateCourseDto dto, CourseService courseService)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
            return TypedResults.BadRequest(new { message = "O nome do curso é obrigatório." });

        var result = await courseService.CreateCourse(dto);

       if (result is null) 
            return TypedResults.Conflict(new { message = "Este curso já está cadastrado." });

        return TypedResults.Created($"/courses/{result.Id}", result);
    }

    private static async Task<IResult> GetCourses(CourseService courseService)
    {
        var courses = await courseService.GetCourses();

        return TypedResults.Ok(courses);
    }

    private static async Task<IResult> UpdateCourse(
        int id, 
        UpdateCourseDto dto, 
        CourseService courseService)
    {
      if (string.IsNullOrWhiteSpace(dto.Name))
            return TypedResults.BadRequest(new { message = "O nome do curso é obrigatório." });

        var result = await courseService.UpdateCourse(id, dto);
        
        if (result is null)
            return TypedResults.NotFound(new { message = "Curso não encontrado." });

        if (result.Value == false)
            return TypedResults.Conflict(new { message = "Este curso já está cadastrado." });

        return TypedResults.NoContent();
    }
}