using ScholarshipPlatform.Authentication.Dtos;

namespace ScholarshipPlatform.Authentication;

public static class AuthenticationEndpoints
{
    public static RouteGroupBuilder MapAuthenticationEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/login", Login);

        group.MapGet("/confirm-email", ConfirmEmail);

        // Reenvia o email de confirmação para contas ainda não confirmadas.
        group.MapPost("/resend-confirmation", ResendEmailConfirmation);

        group.MapPost("/forgot-password", ForgotPassword);

        group.MapPost("/reset-password", ResetPassword);

        return group;
    }

    private static async Task<IResult> Login(
        LoginDto dto, 
        LoginDtoValidator validator,
        AuthenticationService authenticationService)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if(!validationResult.IsValid)
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var token = await authenticationService.Login(dto);

        return token is null
            ? TypedResults.Unauthorized()
            : TypedResults.Ok(new LoginResponseDto(token));
    }

    private static async Task<IResult> ConfirmEmail(
        int userId, 
        string token, 
        AuthenticationService authenticationService)
    {
        var succeeded = await authenticationService.ConfirmEmail(userId, token);

    if (!succeeded) 
                return TypedResults.BadRequest(new {message = "Falha na confirmação do e-mail."});

            return TypedResults.Ok(new {message = "E-mail confirmado com sucesso."});
        }

    private static async Task<IResult> ResendEmailConfirmation(
        ResendEmailConfirmationDto dto,
        AuthenticationService authenticationService)
    {
        var result = await authenticationService.ResendEmailConfirmation(dto);

        //if(result == false)
        //it doesn't matter if the result is false or not, return Ok 
        return TypedResults.Ok(new 
        { 
            message = "Se o e-mail estiver cadastrado e ainda não confirmado, um novo link de confirmação foi enviado." 
        });
    }

    private static async Task<IResult> ForgotPassword(ForgotPasswordDto dto, AuthenticationService authenticationService)
    {
        await authenticationService.ForgotPassword(dto);

        return TypedResults.Ok(new 
        { 
            message = "Se o e-mail estiver cadastrado, um link para redefinição de senha foi enviado." 
        });
    }

    private static async Task<IResult> ResetPassword(ResetPasswordDto dto, AuthenticationService authenticationService)
    {
        var succeeded = await authenticationService.ResetPassword(dto);

        if (!succeeded) 
            return TypedResults.BadRequest(new { message = "Falha ao redefinir a senha." });

        return TypedResults.Ok(new { message = "Senha redefinida com sucesso." });
    }
}