#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Companies.Application.Common.Models;
using Rentar.Modules.Companies.Application.Employees.Commands;
using Rentar.Modules.Companies.Application.Employees.Queries;
using Rentar.Shared.Application.Security;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Presentation.Endpoints.V2;

/// <summary>
/// Employees belong to a company, so they are addressed under it: <c>/companies/{companyId}/employees</c>. Every route
/// needs an access token and a permission of the caller's roles in that company.
/// </summary>
public class EmployeesEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Employees", "companies/{companyId:int}/employees", 2.0)
            .RequireAuthorization()
            // Authentication refuses a missing or invalid token with an empty 401, not a problem.
            .WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status401Unauthorized))
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapPost("/", Add)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);
        group.MapDelete("/{employeeId:int}", Remove)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    /// <summary>One page of the company's employees with their names and roles; needs <c>employees.read</c>.</summary>
    private static async Task<Ok<PagedResponse<EmployeeResponse>>> GetAll(
        IQueryHandler<GetEmployeesQuery, Page<EmployeeResponse>> getEmployees,
        ICurrentUser currentUser,
        int companyId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getEmployees.HandleAsync(
            new GetEmployeesQuery(currentUser.RequireUserId(), companyId, paging.ToRequest()),
            cancellationToken));
    }

    /// <summary>
    /// Employs a user with some of the company's roles; needs <c>employees.manage</c> and every permission those roles grant.
    /// </summary>
    private static async Task<Created<EmployeeResponse>> Add(
        ICommandHandler<AddEmployeeCommand, EmployeeResponse> addEmployee,
        ICurrentUser currentUser,
        HttpRequest request,
        int companyId,
        EmployeeRequest body,
        CancellationToken cancellationToken)
    {
        var employee = await addEmployee.HandleAsync(
            new AddEmployeeCommand(currentUser.RequireUserId(), companyId, body.UserId, body.RoleIds),
            cancellationToken);

        return RestResults.Created(request, employee.Id, employee);
    }

    /// <summary>
    /// Removes an employee, the caller included; needs <c>employees.manage</c> and every permission the employee holds.
    /// The last employee holding every permission can not be removed.
    /// </summary>
    private static async Task<NoContent> Remove(
        ICommandHandler<RemoveEmployeeCommand> removeEmployee,
        ICurrentUser currentUser,
        int companyId,
        int employeeId,
        CancellationToken cancellationToken)
    {
        await removeEmployee.HandleAsync(new RemoveEmployeeCommand(currentUser.RequireUserId(), companyId, employeeId), cancellationToken);
        return TypedResults.NoContent();
    }
}
