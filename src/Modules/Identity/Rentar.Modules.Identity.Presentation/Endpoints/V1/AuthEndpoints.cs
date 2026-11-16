using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Identity.Application;
using Rentar.Modules.Identity.Application.Auth;
using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Modules.Identity.Application.Auth.Queries;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using Rentar.Shared.Presentation.Endpoints;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V1;

public class AuthEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Auth", "auth", 1.0);

        group.MapPost("/login", Login);
        group.MapPost("/checkpassword", CheckPassword);
        group.MapPost("/register", Register);
    }

    /// <summary>
    /// Bad credentials answer the reason as a bare string, not the usual v1 body; v1 clients rely on that.
    /// </summary>
    private static Results<Ok<V1Response<AccessToken>>, BadRequest<string>> Login(
        IQueryHandler<VerifyCredentialsQuery, User> verifyCredentials,
        ICommandHandler<CreateAccessTokenCommand, AccessToken> createAccessToken,
        VerifyCredentialsQuery credentials)
    {
        try
        {
            var user = verifyCredentials.Handle(credentials);
            return V1Results.Ok(createAccessToken.Handle(new CreateAccessTokenCommand(user)), Messages.AccessTokenCreated);
        }
        catch (InvalidCredentialsException e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }

    private static Results<Ok<V1Response<User>>, BadRequest<string>> CheckPassword(
        IQueryHandler<VerifyCredentialsQuery, User> verifyCredentials,
        VerifyCredentialsQuery credentials)
    {
        try
        {
            return V1Results.Ok(verifyCredentials.Handle(credentials), Messages.SuccessfulLogin);
        }
        catch (InvalidCredentialsException e)
        {
            return TypedResults.BadRequest(e.Message);
        }
    }

    private static Ok<V1Response<AccessToken>> Register(
        ICommandHandler<RegisterUserCommand, User> registerUser,
        ICommandHandler<CreateAccessTokenCommand, AccessToken> createAccessToken,
        RegisterUserCommand registration)
    {
        var user = registerUser.Handle(registration);
        return V1Results.Ok(createAccessToken.Handle(new CreateAccessTokenCommand(user)), Messages.AccessTokenCreated);
    }
}
