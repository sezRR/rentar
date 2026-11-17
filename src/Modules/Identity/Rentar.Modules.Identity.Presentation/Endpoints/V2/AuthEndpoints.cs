#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V2;

/// <summary>
/// Logging in creates an access token. Accounts are created through <c>POST /users</c>.
/// </summary>
public class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Auth", "auth", 2.0);

        group.MapPost("/tokens", CreateToken)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    /// <summary>
    /// Bad credentials answer 401 with the same detail whatever was wrong, so the response does not reveal which
    /// accounts exist. The right credentials of an inactive user answer 403.
    /// </summary>
    private static async Task<Ok<AccessToken>> CreateToken(
        IQueryHandler<VerifyCredentialsQuery, User> verifyCredentials,
        ICommandHandler<CreateAccessTokenCommand, AccessToken> createAccessToken,
        VerifyCredentialsQuery credentials,
        CancellationToken cancellationToken)
    {
        var user = await verifyCredentials.HandleAsync(credentials, cancellationToken);
        return TypedResults.Ok(await createAccessToken.HandleAsync(new CreateAccessTokenCommand(user), cancellationToken));
    }
}
