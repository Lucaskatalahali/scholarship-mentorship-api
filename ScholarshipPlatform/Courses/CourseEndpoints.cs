using ScholarshipPlatform.Courses.Dtos;

namespace ScholarshipPlatform.Courses;

public static class CourseEndpoints
{
    public static RouteGroupBuilder MapCourseEndpoints(this WebApplication app)
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
        if(string.IsNullOrWhiteSpace(dto.Name))
            return TypedResults.BadRequest(new { message = "Course name is required" });

        var result = await courseService.CreateCourse(dto);

        if(result is null) return TypedResults.Conflict("This course is already registered.");

        return TypedResults.Created();
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
        if(string.IsNullOrWhiteSpace(dto.Name))
            return TypedResults.BadRequest(new { message = "Course name is required" });

        var result = await courseService.UpdateCourse(id, dto);

        if(result is null) return 
            TypedResults.NotFound("Course Id not found.");

        if(result.Value == false) 
            TypedResults.Conflict("This course is already registered");

        return TypedResults.NoContent();
    }
}