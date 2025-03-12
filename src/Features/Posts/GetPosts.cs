namespace NetAPI.Features.Posts;

using NetAPI.Common.Api;

public class GetPosts: IEndpoint
{
    public record Request(string Title, string? Content);
    public record PostList(int Id);

    public static void Map(IEndpointRouteBuilder app) => app
        .MapGet("/", Handle)
        .WithSummary("Gets all posts");

    private static PostList Handle([AsParameters] Request request, CancellationToken cancellationToken)
    {
        return new PostList(1);
    }

}