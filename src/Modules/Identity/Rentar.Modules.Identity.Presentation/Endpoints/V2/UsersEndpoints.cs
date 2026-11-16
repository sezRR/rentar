#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Identity.Application.Auth.Commands;
using Rentar.Modules.Identity.Application.Users.Commands;
using Rentar.Modules.Identity.Application.Users.Queries;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V2;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Users", "users", 2.0);

        group.MapGet("/", GetAll)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Lists users, optionally only the one with an email. The full list is closed during the 22:00 maintenance hour.
    /// </summary>
    private static async Task<Ok<List<UserResponse>>> GetAll(
        IQueryHandler<GetUsersQuery, List<User>> getUsers,
        IQueryHandler<GetUserByEmailQuery, User> getUserByEmail,
        string? email,
        CancellationToken cancellationToken)
    {
        if (email is not null)
        {
            var user = await getUserByEmail.HandleAsync(new GetUserByEmailQuery(email), cancellationToken);
            return TypedResults.Ok(user is null ? new List<UserResponse>() : [UserResponse.From(user)]);
        }

        var users = await getUsers.HandleAsync(new GetUsersQuery(), cancellationToken);
        return TypedResults.Ok(users.ConvertAll(UserResponse.From));
    }

    private static async Task<Results<Ok<UserResponse>, ProblemHttpResult>> GetById(
        IQueryHandler<GetUserByIdQuery, User> getUserById,
        int id,
        CancellationToken cancellationToken)
    {
        var user = await getUserById.HandleAsync(new GetUserByIdQuery(id), cancellationToken);
        return user is null ? RestResults.NotFound("User", id) : TypedResults.Ok(UserResponse.From(user));
    }

    /// <summary>
    /// Creates an account; an email that is already registered answers 409. Log in through
    /// <c>POST /auth/tokens</c> to get an access token for it.
    /// </summary>
    private static async Task<Created<UserResponse>> Create(
        ICommandHandler<RegisterUserCommand, User> registerUser,
        HttpRequest request,
        UserRequest body,
        CancellationToken cancellationToken)
    {
        var user = await registerUser.HandleAsync(
            new RegisterUserCommand(body.FirstName, body.LastName, body.Email, body.Password),
            cancellationToken);
        return RestResults.Created(request, user.Id, UserResponse.From(user));
    }

    /// <summary>
    /// Replaces the user; an email another user already has answers 409.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Update(
        IQueryHandler<GetUserByIdQuery, User> getUserById,
        ICommandHandler<UpdateUserCommand, User> updateUser,
        int id,
        UserRequest body,
        CancellationToken cancellationToken)
    {
        if (await getUserById.HandleAsync(new GetUserByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("User", id);
        }

        await updateUser.HandleAsync(
            new UpdateUserCommand(id, body.FirstName, body.LastName, body.Email, body.Password),
            cancellationToken);
        return TypedResults.NoContent();
    }
}
