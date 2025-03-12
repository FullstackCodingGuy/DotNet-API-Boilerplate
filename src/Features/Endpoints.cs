namespace NetAPI.Features;

using NetAPI.Features.Posts;

public static class FeaturesEndpointsExtension {
    public static void MapFeatureEndpoints(this WebApplication app) {
         var endpoint = app.MapPublicGroup("/tasks").WithTags("Posts");
        endpoint
        .MapEndpoint<GetPosts>()
        .MapEndpoint<CreatePost>();
    }
}