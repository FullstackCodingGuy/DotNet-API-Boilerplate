namespace NetAPI.Features.Posts;

using NetAPI.Common.Api;

public class GetPosts: IEndpoint
{
    public record Request(string Title, string? Content);
    public record Response(int Id);

    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithSummary("Gets all posts");

    private static Response Handle([AsParameters] Request request, CancellationToken cancellationToken)
    {
        return new Response(1);
    }

}