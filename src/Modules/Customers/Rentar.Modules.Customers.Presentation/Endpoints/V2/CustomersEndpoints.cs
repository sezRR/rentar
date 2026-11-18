#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Customers.Application.Commands;
using Rentar.Modules.Customers.Application.Queries;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Presentation.Endpoints.V2;

public class CustomersEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Customers", "customers", 2.0);

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Lists customers, optionally only those of a user.
    /// </summary>
    private static async Task<Ok<List<Customer>>> GetAll(
        IQueryHandler<GetCustomersQuery, List<Customer>> getCustomers,
        IQueryHandler<GetCustomersByUserQuery, List<Customer>> getCustomersByUser,
        int? userId,
        CancellationToken cancellationToken)
    {
        var customers = userId is { } user
            ? await getCustomersByUser.HandleAsync(new GetCustomersByUserQuery(user), cancellationToken)
            : await getCustomers.HandleAsync(new GetCustomersQuery(), cancellationToken);

        return TypedResults.Ok(customers);
    }

    private static async Task<Results<Ok<Customer>, ProblemHttpResult>> GetById(
        IQueryHandler<GetCustomerByIdQuery, Customer> getCustomerById,
        int id,
        CancellationToken cancellationToken)
    {
        var customer = await getCustomerById.HandleAsync(new GetCustomerByIdQuery(id), cancellationToken);
        return customer is null ? RestResults.NotFound("Customer", id) : TypedResults.Ok(customer);
    }

    private static async Task<Created<Customer>> Create(
        ICommandHandler<CreateCustomerCommand, Customer> createCustomer,
        HttpRequest request,
        CreateCustomerCommand command,
        CancellationToken cancellationToken)
    {
        var customer = await createCustomer.HandleAsync(command, cancellationToken);
        return RestResults.Created(request, customer.Id, customer);
    }
}
