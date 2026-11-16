using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Application.Brands.Commands;
using Rentar.Modules.Fleet.Application.Brands.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;

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

    private static Ok<V1Response<List<Brand>>> GetAll(IQueryHandler<GetBrandsQuery, List<Brand>> getBrands)
    {
        return V1Results.Ok(getBrands.Handle(new GetBrandsQuery()), Messages.BrandsListed);
    }

    private static Ok<V1Response<Brand>> GetById(IQueryHandler<GetBrandByIdQuery, Brand> getBrandById, int id)
    {
        return V1Results.Ok(getBrandById.Handle(new GetBrandByIdQuery(id)), null);
    }

    private static Ok<V1Response> Add(ICommandHandler<CreateBrandCommand, Brand> createBrand, CreateBrandCommand command)
    {
        createBrand.Handle(command);
        return V1Results.Ok(Messages.BrandAdded);
    }

    private static Ok<V1Response> Update(ICommandHandler<UpdateBrandCommand> updateBrand, UpdateBrandCommand command)
    {
        updateBrand.Handle(command);
        return V1Results.Ok(Messages.BrandUpdated);
    }

    private static Ok<V1Response> Delete(ICommandHandler<DeleteBrandCommand> deleteBrand, DeleteBrandCommand command)
    {
        deleteBrand.Handle(command);
        return V1Results.Ok(Messages.BrandDeleted);
    }
}
