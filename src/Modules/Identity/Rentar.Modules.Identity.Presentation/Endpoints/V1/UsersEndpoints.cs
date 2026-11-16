using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Identity.Application;
using Rentar.Modules.Identity.Application.Users.Commands;
using Rentar.Modules.Identity.Application.Users.Queries;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V1;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Users", "users", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getbymail", GetByMail);
        group.MapGet("/getuserfindeksrating", GetUserFindeksRating);
        group.MapGet("/getuserbyid", GetUserById);
        group.MapPost("/add", Add);
        group.MapPost("/update", Update);
    }

    private static Ok<V1Response<List<User>>> GetAll(IQueryHandler<GetUsersQuery, List<User>> getUsers)
    {
        return V1Results.Ok(getUsers.Handle(new GetUsersQuery()), Messages.UsersListed);
    }

    private static Ok<V1Response<User>> GetByMail(IQueryHandler<GetUserByEmailQuery, User> getUserByEmail, string email)
    {
        return V1Results.Ok(getUserByEmail.Handle(new GetUserByEmailQuery(email)), null);
    }

    private static Ok<V1Response<List<User>>> GetUserFindeksRating(
        IQueryHandler<GetUsersByMinFindeksRatingQuery, List<User>> getUsersByMinFindeksRating,
        int findeksRating)
    {
        return V1Results.Ok(getUsersByMinFindeksRating.Handle(new GetUsersByMinFindeksRatingQuery(findeksRating)), null);
    }

    private static Ok<V1Response<User>> GetUserById(IQueryHandler<GetUserByIdQuery, User> getUserById, int id)
    {
        return V1Results.Ok(getUserById.Handle(new GetUserByIdQuery(id)), null);
    }

    private static Ok<V1Response> Add(ICommandHandler<AddUserCommand> addUser, AddUserCommand command)
    {
        addUser.Handle(command);
        return V1Results.Ok(Messages.UserAdded);
    }

    private static Ok<V1Response<User>> Update(ICommandHandler<UpdateUserCommand, User> updateUser, UpdateUserCommand command)
    {
        return V1Results.Ok(updateUser.Handle(command), Messages.UserUpdated);
    }
}
