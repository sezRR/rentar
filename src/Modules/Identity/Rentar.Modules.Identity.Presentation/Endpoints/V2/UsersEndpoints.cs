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
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V2;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Users", "users", 2.0);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapPost("/{id:int}/deactivation", Deactivate)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    /// <summary>
    /// Lists one page of users, optionally only the one with an email.
    /// </summary>
    private static async Task<Ok<PagedResponse<UserResponse>>> GetAll(
        IQueryHandler<GetUsersQuery, Page<User>> getUsers,
        IQueryHandler<GetUsersByEmailQuery, Page<User>> getUsersByEmail,
        string? email,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        var users = email is not null
            ? await getUsersByEmail.HandleAsync(new GetUsersByEmailQuery(email, paging.ToRequest()), cancellationToken)
            : await getUsers.HandleAsync(new GetUsersQuery(paging.ToRequest()), cancellationToken);

        return RestResults.Paged(users.Map(UserResponse.From));
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
    /// Replaces the user; an email another user already has answers 409. Whether the user is active stays as it is.
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

    /// <summary>
    /// Deactivates the user: they can no longer sign in, and right after they stop working at every company and their
    /// rentals that have not started are cancelled. A user who is the last one holding every permission at a company answers 409, as
    /// that company would be left without anyone able to run it. Deactivating an inactive user changes nothing.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Deactivate(
        IQueryHandler<GetUserByIdQuery, User> getUserById,
        ICommandHandler<DeactivateUserCommand> deactivateUser,
        int id,
        CancellationToken cancellationToken)
    {
        if (await getUserById.HandleAsync(new GetUserByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("User", id);
        }

        await deactivateUser.HandleAsync(new DeactivateUserCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
