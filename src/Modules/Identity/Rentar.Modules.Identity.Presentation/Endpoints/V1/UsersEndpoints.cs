using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Identity.Application;
using Rentar.Modules.Identity.Application.Users.Commands;
using Rentar.Modules.Identity.Application.Users.Queries;
using Rentar.Modules.Identity.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Identity.Presentation.Endpoints.V1;

public class UsersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Users", "users", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getbymail", GetByMail);
        group.MapGet("/getuserbyid", GetUserById);
        group.MapPost("/add", Add);
        group.MapPost("/update", Update);
    }

    private static async Task<Ok<V1Response<List<User>>>> GetAll(IQueryHandler<GetUsersQuery, Page<User>> getUsers, CancellationToken cancellationToken)
    {
        return V1Results.Ok((await getUsers.HandleAsync(new GetUsersQuery(), cancellationToken)).Items, Messages.UsersListed);
    }

    private static async Task<Ok<V1Response<User>>> GetByMail(
        IQueryHandler<GetUserByEmailQuery, User> getUserByEmail,
        string email,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getUserByEmail.HandleAsync(new GetUserByEmailQuery(email), cancellationToken), null);
    }

    private static async Task<Ok<V1Response<User>>> GetUserById(IQueryHandler<GetUserByIdQuery, User> getUserById, int id, CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getUserById.HandleAsync(new GetUserByIdQuery(id), cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Add(ICommandHandler<AddUserCommand> addUser, AddUserCommand command, CancellationToken cancellationToken)
    {
        await addUser.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.UserAdded);
    }

    private static async Task<Ok<V1Response<User>>> Update(
        ICommandHandler<UpdateUserCommand, User> updateUser,
        UpdateUserCommand command,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await updateUser.HandleAsync(command, cancellationToken), Messages.UserUpdated);
    }
}
