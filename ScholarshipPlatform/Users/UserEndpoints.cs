namespace ScholarshipPlatform.Users;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoins(this WebApplication app)
    {
        var group = app.MapGroup("/users");

        group.MapGet("/{id}", GetUser);
        group.MapGet("/", GetAllUsers);
        group.MapPost("/", CreateUser);
        group.MapPatch("/{id}", PatchUser);   
        group.MapDelete("/{id}", DeleteUser);

        return group;    
    }

    private static async Task<IResult> CreateUser(CreateUserDto dto, CreateUserDtoValidator validator, UserService userService)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return  TypedResults.ValidationProblem(validationResult.ToDictionary());

        var result = await userService.CreateUser(dto);

        if (!result.IsSuccess)
        {
            return TypedResults.ValidationProblem(result.Errors!);
        }

        return TypedResults.Created($"/users/{result.Data!.Id}", result.Data); //Data is userDto
    }

    private static async Task<IResult> GetUser(int id, UserService userService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var userDto = await userService.GetUser(id);

        return userDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(userDto);
    }

    private static async Task<IResult> GetAllUsers(UserService userService)
    {
        var usersDto = await userService.GetAllUsers();

        return TypedResults.Ok(usersDto);
    }

    private static async Task<IResult> PatchUser(int id, PatchUserDto dto, PatchUserDtoValidator validator, UserService userService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var WasUpdated = await userService.PatchUser(id, dto);

        return WasUpdated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    private static async Task<IResult> DeleteUser(int id, UserService userService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var wasDeleted = await userService.DeleteUser(id);

        return wasDeleted
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }
}