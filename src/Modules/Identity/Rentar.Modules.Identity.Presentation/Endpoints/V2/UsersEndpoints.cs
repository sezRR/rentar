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
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Lists users, optionally only the one with an email and/or those with at least a Findeks rating.
    /// The full list is closed during the 22:00 maintenance hour.
    /// </summary>
    private static Ok<List<UserResponse>> GetAll(
        IQueryHandler<GetUsersQuery, List<User>> getUsers,
        IQueryHandler<GetUserByEmailQuery, User> getUserByEmail,
        IQueryHandler<GetUsersByMinFindeksRatingQuery, List<User>> getUsersByMinFindeksRating,
        string? email,
        int? minFindeksRating)
    {
        if (email is not null)
        {
            var user = getUserByEmail.Handle(new GetUserByEmailQuery(email));
            var matches = new List<UserResponse>();

            if (user is not null && user.FindeksRating >= (minFindeksRating ?? int.MinValue))
            {
                matches.Add(UserResponse.From(user));
            }

            return TypedResults.Ok(matches);
        }

        var users = minFindeksRating is { } rating
            ? getUsersByMinFindeksRating.Handle(new GetUsersByMinFindeksRatingQuery(rating))
            : getUsers.Handle(new GetUsersQuery());

        return TypedResults.Ok(users.ConvertAll(UserResponse.From));
    }

    private static Results<Ok<UserResponse>, ProblemHttpResult> GetById(IQueryHandler<GetUserByIdQuery, User> getUserById, int id)
    {
        var user = getUserById.Handle(new GetUserByIdQuery(id));
        return user is null ? RestResults.NotFound("User", id) : TypedResults.Ok(UserResponse.From(user));
    }

    /// <summary>
    /// Creates an account; an email that is already registered answers 409. Log in through
    /// <c>POST /auth/tokens</c> to get an access token for it.
    /// </summary>
    private static Created<UserResponse> Create(ICommandHandler<RegisterUserCommand, User> registerUser, HttpRequest request, UserRequest body)
    {
        var user = registerUser.Handle(new RegisterUserCommand(body.FirstName, body.LastName, body.Email, body.Password, body.FindeksRating));
        return RestResults.Created(request, user.Id, UserResponse.From(user));
    }

    private static Results<NoContent, ProblemHttpResult> Update(
        IQueryHandler<GetUserByIdQuery, User> getUserById,
        ICommandHandler<UpdateUserCommand, User> updateUser,
        int id,
        UserRequest body)
    {
        if (getUserById.Handle(new GetUserByIdQuery(id)) is null)
        {
            return RestResults.NotFound("User", id);
        }

        updateUser.Handle(new UpdateUserCommand(id, body.FirstName, body.LastName, body.Email, body.Password, body.FindeksRating));
        return TypedResults.NoContent();
    }
}
