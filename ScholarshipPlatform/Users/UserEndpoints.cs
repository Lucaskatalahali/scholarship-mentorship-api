using System.Security.Claims;
using ScholarshipPlatform.Users.Dtos;

namespace ScholarshipPlatform.Users;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/users");

        group.MapGet("/", GetUsers)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));
        
        group.MapGet("/{id:int}", GetUserById)
            .RequireAuthorization(policy => policy.RequireRole("Admin", "Mentor"));

        group.MapGet("/me", GetCurrentUser)
            .RequireAuthorization();

        group.MapPost("/", RegisterUser);

        group.MapPatch("/{id:int}/approve-registration", ApproveRegistration)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPatch("/{id:int}", UpdateUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));   

        group.MapPatch("/me", UpdateCurrentUser)
            .RequireAuthorization();

        //Apenas Admin pode remover um usuário
        group.MapDelete("/{id:int}", DeleteUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        group.MapPatch("/{userEmail}/suspend", SuspendUserVoluntarily)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

         group.MapPatch("/{userEmail}/reactivate", ReactivateUser)
            .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;    
    }

    private static async Task<IResult> RegisterUser(
        CreateUserDto dto, 
        CreateUserDtoValidator validator, 
        UserService userService)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return  TypedResults.ValidationProblem(validationResult.ToDictionary());

        var result = await userService.RegisterUser(dto);

        if (!result.IsSuccess)
        {
            return TypedResults.ValidationProblem(result.Errors!);
        }

        return TypedResults.Created($"/users/{result.Data!.Id}", result.Data); //Data is userDto
        //Após o registro, informar o user pra verificar seu email (talvez pelo frontend)
    }

    private static async Task<IResult> ApproveRegistration(int id, UserService userService)
    {
        var result = await userService.ApproveRegistration(id);

        return result switch
        {
        ApprovalResult.Success => 
            TypedResults.NoContent(),

        ApprovalResult.NotFound => 
            TypedResults.NotFound(),

        ApprovalResult.EmailNotConfirmed => 
            TypedResults.BadRequest(new 
            { 
                code = "EmailNotConfirmed", 
                message = "Não é possível aprovar o registo: o utilizador ainda não confirmou o seu endereço de e-mail." 
            }),

        ApprovalResult.AlreadyApproved => 
            TypedResults.Conflict("Este utilizador já se encontra aprovado e ativo."),

        ApprovalResult.InvalidStatus => 
            TypedResults.BadRequest("Apenas utilizadores com estado 'RegistrationPending' podem ser aprovados."),

        _ => TypedResults.BadRequest("Erro ao processar a aprovação do registo.")
        };
    }

    private static async Task<IResult> GetUserById(int id, UserService userService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var userDto = await userService.GetUserById(id);

        return userDto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(userDto);
    }

private static async Task<IResult> GetCurrentUser(
    ClaimsPrincipal user, 
    UserService userService)
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

    if (!int.TryParse(userId, out var id))
        return TypedResults.Unauthorized();

    var userDto = await userService.GetUserById(id);

    return userDto is null
        ? TypedResults.NotFound()
        : TypedResults.Ok(userDto);
}

    private static async Task<IResult> GetUsers(UserService userService)
    {
        var usersDto = await userService.GetUsers();
        return TypedResults.Ok(usersDto);
    }

    //O Admin edita as infos do usuário
    private static async Task<IResult> UpdateUser(
        int id, 
        PatchUserDto dto, 
        PatchUserDtoValidator validator, 
        UserService userService)
    {
        if(id <= 0) return TypedResults.BadRequest("ID must be greater than 0");

        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var wasUpdated = await userService.UpdateUser(id, dto);

        return wasUpdated
            ? TypedResults.NoContent()
            : TypedResults.NotFound();
    }

    //O usuário edita suas próprias informações
    private static async Task<IResult> UpdateCurrentUser(
        ClaimsPrincipal user,
        PatchUserDto dto, 
        PatchUserDtoValidator validator,
        UserService userService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if(!int.TryParse(userId, out var id))
            return TypedResults.Unauthorized();   

        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var wasUpdated = await userService.UpdateUser(id, dto);

        return wasUpdated
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

    private static async Task<IResult> SuspendUserVoluntarily(string userEmail, UserService userService)
    {
        var result = await userService.SuspendUser(userEmail);

        if(result is null) 
            return TypedResults.NotFound();

        if(result.Value == false) 
            return TypedResults.Problem(
                "Account has already been suspended or something went wrong while updating");

        return TypedResults.NoContent(); 
    }

    private static async Task<IResult> ReactivateUser(string userEmail, UserService userService)
    {
        var result = await userService.ReactivateUser(userEmail);

        if(result is null) 
            return TypedResults.NotFound();

        if(result.Value == false) 
            return TypedResults.Problem(
                "Account is already active or something went wrong while updating");

        return TypedResults.NoContent(); 
    }
}