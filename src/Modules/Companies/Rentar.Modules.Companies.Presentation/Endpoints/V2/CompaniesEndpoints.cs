#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Companies.Application.Companies.Commands;
using Rentar.Modules.Companies.Application.Companies.Queries;
using Rentar.Modules.Companies.Application.Roles.Queries;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Security;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Presentation.Endpoints.V2;

/// <summary>
/// Every route needs an access token: what a caller may see and do depends on which companies they work at and their
/// roles there. A company the caller does not work at answers 404, as if it did not exist.
/// </summary>
public class CompaniesEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Companies", "companies", 2.0)
            .RequireAuthorization()
            // Authentication refuses a missing or invalid token with an empty 401, not a problem.
            .WithMetadata(new ProducesResponseTypeMetadata(StatusCodes.Status401Unauthorized));

        group.MapPost("/", Register)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status400BadRequest);
        group.MapGet("/", GetMine)
            .ProducesValidationProblem();
        group.MapGet("/{companyId:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPut("/{companyId:int}", Update)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapGet("/{companyId:int}/roles", GetRoles)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Registers a company with the predefined roles (Owner, Admin, Member); the caller becomes its Owner.
    /// </summary>
    private static async Task<Created<Company>> Register(
        ICommandHandler<RegisterCompanyCommand, Company> registerCompany,
        ICurrentUser currentUser,
        HttpRequest request,
        CompanyRequest body,
        CancellationToken cancellationToken)
    {
        var company = await registerCompany.HandleAsync(new RegisterCompanyCommand(currentUser.RequireUserId(), body.Name), cancellationToken);
        return RestResults.Created(request, company.Id, company);
    }

    /// <summary>One page of the companies the caller works at.</summary>
    private static async Task<Ok<PagedResponse<Company>>> GetMine(
        IQueryHandler<GetCompaniesByUserQuery, Page<Company>> getCompaniesByUser,
        ICurrentUser currentUser,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getCompaniesByUser.HandleAsync(
            new GetCompaniesByUserQuery(currentUser.RequireUserId(), paging.ToRequest()),
            cancellationToken));
    }

    private static async Task<Ok<Company>> GetById(
        IQueryHandler<GetCompanyByIdQuery, Company> getCompanyById,
        ICurrentUser currentUser,
        int companyId,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await getCompanyById.HandleAsync(new GetCompanyByIdQuery(currentUser.RequireUserId(), companyId), cancellationToken));
    }

    /// <summary>Renames the company; needs <c>company.manage</c>.</summary>
    private static async Task<NoContent> Update(
        ICommandHandler<UpdateCompanyCommand> updateCompany,
        ICurrentUser currentUser,
        int companyId,
        CompanyRequest body,
        CancellationToken cancellationToken)
    {
        await updateCompany.HandleAsync(new UpdateCompanyCommand(currentUser.RequireUserId(), companyId, body.Name), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>One page of the company's roles and the permissions each grants; open to every employee.</summary>
    private static async Task<Ok<PagedResponse<Role>>> GetRoles(
        IQueryHandler<GetRolesQuery, Page<Role>> getRoles,
        ICurrentUser currentUser,
        int companyId,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getRoles.HandleAsync(
            new GetRolesQuery(currentUser.RequireUserId(), companyId, paging.ToRequest()),
            cancellationToken));
    }
}
