using Microsoft.AspNetCore.Mvc;
using ScholarshipPlatform.SuccessStories.Dtos;

namespace ScholarshipPlatform.SuccessStories;

public static class SuccessStoryEndpoints
{
    public static RouteGroupBuilder MapSuccessStoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/success-stories");  

            group.MapGet("/", GetSuccessStories);

            group.MapGet("/featured", GetFeaturedSuccessStories);

            group.MapPost("/", CreateSuccessStory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .DisableAntiforgery();

            group.MapPatch("/{id:int}", UpdateSuccessStory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"))
                .DisableAntiforgery();
            
            group.MapDelete("/{id:int}", DeleteSuccessStory)
                .RequireAuthorization(policy => policy.RequireRole("Admin"));

        return group;
    }

    private static async Task<IResult> CreateSuccessStory(
        [FromForm] CreateSuccessStoryDto dto, 
        CreateSuccessStoryDtoValidator validator, 
        SuccessStoryService successStoryService
    )
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid) 
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var responseDto = await successStoryService.CreateSuccessStory(dto);
        return TypedResults.Ok(responseDto);
    }

    private static async Task<IResult> GetSuccessStories(SuccessStoryService successStoryService)
    {
        var storiesDto = await successStoryService.GetSuccessStories();
        return TypedResults.Ok(storiesDto);
    }

    private static async Task<IResult> GetFeaturedSuccessStories(SuccessStoryService successStoryService)
    {
        var FeaturedStoriesDto = await successStoryService.GetFeaturedSuccessStories();
        return TypedResults.Ok(FeaturedStoriesDto);
    }

    private static async Task<IResult> UpdateSuccessStory(
        int id, 
        [FromForm] PatchSuccessStoryDto dto, 
        PatchSuccessStoryDtoValidator validator, 
        SuccessStoryService successStoryService
    )
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (!validationResult.IsValid) 
            return TypedResults.ValidationProblem(validationResult.ToDictionary());

        var result = await successStoryService.UpdateSuccessStory(id, dto);

        if (result is null) 
            return TypedResults.NotFound(new { message = "História de sucesso não encontrada." });

        return TypedResults.NoContent();
    }

    private static async Task<IResult> DeleteSuccessStory(int id, SuccessStoryService successStoryService)
    {
        if (id <= 0)
            return TypedResults.BadRequest(new { message = "O ID deve ser maior que 0." });

        var result = await successStoryService.DeleteSuccessStory(id);

        if (result is null)
            return TypedResults.NotFound(new { message = "História de sucesso não encontrada." });
        
        return TypedResults.NoContent();
    }
}