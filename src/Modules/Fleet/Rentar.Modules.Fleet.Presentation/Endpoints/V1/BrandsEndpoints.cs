using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Application.Brands.Commands;
using Rentar.Modules.Fleet.Application.Brands.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V1;

public class BrandsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Brands", "brands", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getbyid", GetById);
        group.MapPost("/add", Add);
        group.MapPost("/update", Update);
        group.MapPost("/delete", Delete);
    }

    private static async Task<Ok<V1Response<List<Brand>>>> GetAll(IQueryHandler<GetBrandsQuery, Page<Brand>> getBrands, CancellationToken cancellationToken)
    {
        return V1Results.Ok((await getBrands.HandleAsync(new GetBrandsQuery(), cancellationToken)).Items, Messages.BrandsListed);
    }

    private static async Task<Ok<V1Response<Brand>>> GetById(IQueryHandler<GetBrandByIdQuery, Brand> getBrandById, int id, CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getBrandById.HandleAsync(new GetBrandByIdQuery(id), cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Add(ICommandHandler<CreateBrandCommand, Brand> createBrand, CreateBrandCommand command, CancellationToken cancellationToken)
    {
        await createBrand.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.BrandAdded);
    }

    private static async Task<Ok<V1Response>> Update(ICommandHandler<UpdateBrandCommand> updateBrand, UpdateBrandCommand command, CancellationToken cancellationToken)
    {
        await updateBrand.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.BrandUpdated);
    }

    private static async Task<Ok<V1Response>> Delete(ICommandHandler<DeleteBrandCommand> deleteBrand, DeleteBrandCommand command, CancellationToken cancellationToken)
    {
        await deleteBrand.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.BrandDeleted);
    }
}
