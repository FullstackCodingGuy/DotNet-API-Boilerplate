namespace NetAPI.Features.Posts;

using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;
using NetAPI.Common.Api;
public class CreatePost : IEndpoint
{
    public static void Map(IEndpointRouteBuilder app) => app
        .MapPost("/", Handle)
        .WithSummary("Creates a new post");

    public record Request(string Title, string? Content);
    public record CreatedResponse(int Id);

    private static async Task<Ok<CreatedResponse>> Handle(Request request, ClaimsPrincipal claimsPrincipal, CancellationToken cancellationToken)
    {
        var response = new CreatedResponse(2);
        return TypedResults.Ok(response);
    }
}