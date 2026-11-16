#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.Brands.Commands;
using Rentar.Modules.Fleet.Application.Brands.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

public class BrandsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Brands", "brands", 2.0);

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem();
        group.MapPut("/{id:int}", Update)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static Ok<List<Brand>> GetAll(IQueryHandler<GetBrandsQuery, List<Brand>> getBrands)
    {
        return TypedResults.Ok(getBrands.Handle(new GetBrandsQuery()));
    }

    private static Results<Ok<Brand>, ProblemHttpResult> GetById(IQueryHandler<GetBrandByIdQuery, Brand> getBrandById, int id)
    {
        var brand = getBrandById.Handle(new GetBrandByIdQuery(id));
        return brand is null ? RestResults.NotFound("Brand", id) : TypedResults.Ok(brand);
    }

    private static Created<Brand> Create(ICommandHandler<CreateBrandCommand, Brand> createBrand, HttpRequest request, BrandRequest body)
    {
        var brand = createBrand.Handle(new CreateBrandCommand(body.BrandName));
        return RestResults.Created(request, brand.Id, brand);
    }

    private static Results<NoContent, ProblemHttpResult> Update(
        IQueryHandler<GetBrandByIdQuery, Brand> getBrandById,
        ICommandHandler<UpdateBrandCommand> updateBrand,
        int id,
        BrandRequest body)
    {
        if (getBrandById.Handle(new GetBrandByIdQuery(id)) is null)
        {
            return RestResults.NotFound("Brand", id);
        }

        updateBrand.Handle(new UpdateBrandCommand(id, body.BrandName));
        return TypedResults.NoContent();
    }

    private static Results<NoContent, ProblemHttpResult> Delete(
        IQueryHandler<GetBrandByIdQuery, Brand> getBrandById,
        ICommandHandler<DeleteBrandCommand> deleteBrand,
        int id)
    {
        if (getBrandById.Handle(new GetBrandByIdQuery(id)) is null)
        {
            return RestResults.NotFound("Brand", id);
        }

        deleteBrand.Handle(new DeleteBrandCommand(id));
        return TypedResults.NoContent();
    }
}
