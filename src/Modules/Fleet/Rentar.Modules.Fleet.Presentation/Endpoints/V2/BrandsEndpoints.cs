#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.Brands.Commands;
using Rentar.Modules.Fleet.Application.Brands.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

public class BrandsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Brands", "brands", 2.0);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem();
        group.MapPut("/{id:int}", Update)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<PagedResponse<Brand>>> GetAll(
        IQueryHandler<GetBrandsQuery, Page<Brand>> getBrands,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getBrands.HandleAsync(new GetBrandsQuery(paging.ToRequest()), cancellationToken));
    }

    private static async Task<Results<Ok<Brand>, ProblemHttpResult>> GetById(
        IQueryHandler<GetBrandByIdQuery, Brand> getBrandById,
        int id,
        CancellationToken cancellationToken)
    {
        var brand = await getBrandById.HandleAsync(new GetBrandByIdQuery(id), cancellationToken);
        return brand is null ? RestResults.NotFound("Brand", id) : TypedResults.Ok(brand);
    }

    private static async Task<Created<Brand>> Create(
        ICommandHandler<CreateBrandCommand, Brand> createBrand,
        HttpRequest request,
        BrandRequest body,
        CancellationToken cancellationToken)
    {
        var brand = await createBrand.HandleAsync(new CreateBrandCommand(body.BrandName), cancellationToken);
        return RestResults.Created(request, brand.Id, brand);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Update(
        IQueryHandler<GetBrandByIdQuery, Brand> getBrandById,
        ICommandHandler<UpdateBrandCommand> updateBrand,
        int id,
        BrandRequest body,
        CancellationToken cancellationToken)
    {
        if (await getBrandById.HandleAsync(new GetBrandByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Brand", id);
        }

        await updateBrand.HandleAsync(new UpdateBrandCommand(id, body.BrandName), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes the brand; a brand that cars still have answers 409.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
        IQueryHandler<GetBrandByIdQuery, Brand> getBrandById,
        ICommandHandler<DeleteBrandCommand> deleteBrand,
        int id,
        CancellationToken cancellationToken)
    {
        if (await getBrandById.HandleAsync(new GetBrandByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Brand", id);
        }

        await deleteBrand.HandleAsync(new DeleteBrandCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
