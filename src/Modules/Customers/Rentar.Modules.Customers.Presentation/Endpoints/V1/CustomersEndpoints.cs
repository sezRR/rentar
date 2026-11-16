using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Customers.Application;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Application.Queries;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Presentation.Endpoints.V1;

public class CustomersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Customers", "customers", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getcustomersbyuserid", GetCustomersByUserId);
        group.MapPost("/add", Add);
    }

    private static async Task<Ok<V1Response<List<Customer>>>> GetAll(
        IQueryHandler<GetCustomersQuery, List<Customer>> getCustomers,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCustomers.HandleAsync(new GetCustomersQuery(), cancellationToken), Messages.CustomersListed);
    }

    private static async Task<Ok<V1Response<List<Customer>>>> GetCustomersByUserId(
        IQueryHandler<GetCustomersByUserQuery, List<Customer>> getCustomersByUser,
        int userId,
        CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getCustomersByUser.HandleAsync(new GetCustomersByUserQuery(userId), cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Add(
        ICommandHandler<CreateCustomerCommand, Customer> createCustomer,
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        await createCustomer.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.CustomerAdded);
    }
}
