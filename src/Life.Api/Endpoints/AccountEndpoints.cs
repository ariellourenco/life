namespace Life.Api.Endpoints;

internal static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/account");

        group.MapIdentityApi<Gamer>();

        return group.WithTags("Account");
    }
}
