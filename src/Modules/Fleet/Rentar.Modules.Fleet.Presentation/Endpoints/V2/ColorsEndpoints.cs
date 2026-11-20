#nullable enable
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application.Colors.Commands;
using Rentar.Modules.Fleet.Application.Colors.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V2;

public class ColorsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Colors", "colors", 2.0);

        group.MapGet("/", GetAll)
            .ProducesValidationProblem();
        group.MapGet("/{id:int}", GetById)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapPost("/", Create)
            .ProducesValidationProblem();
        group.MapPut("/{id:int}", Update)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
        group.MapDelete("/{id:int}", Delete)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict);
    }

    private static async Task<Ok<PagedResponse<Color>>> GetAll(
        IQueryHandler<GetColorsQuery, Page<Color>> getColors,
        [AsParameters] PageParameters paging,
        CancellationToken cancellationToken)
    {
        return RestResults.Paged(await getColors.HandleAsync(new GetColorsQuery(paging.ToRequest()), cancellationToken));
    }

    private static async Task<Results<Ok<Color>, ProblemHttpResult>> GetById(
        IQueryHandler<GetColorByIdQuery, Color> getColorById,
        int id,
        CancellationToken cancellationToken)
    {
        var color = await getColorById.HandleAsync(new GetColorByIdQuery(id), cancellationToken);
        return color is null ? RestResults.NotFound("Color", id) : TypedResults.Ok(color);
    }

    private static async Task<Created<Color>> Create(
        ICommandHandler<CreateColorCommand, Color> createColor,
        HttpRequest request,
        ColorRequest body,
        CancellationToken cancellationToken)
    {
        var color = await createColor.HandleAsync(new CreateColorCommand(body.ColorName), cancellationToken);
        return RestResults.Created(request, color.Id, color);
    }

    private static async Task<Results<NoContent, ProblemHttpResult>> Update(
        IQueryHandler<GetColorByIdQuery, Color> getColorById,
        ICommandHandler<UpdateColorCommand> updateColor,
        int id,
        ColorRequest body,
        CancellationToken cancellationToken)
    {
        if (await getColorById.HandleAsync(new GetColorByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Color", id);
        }

        await updateColor.HandleAsync(new UpdateColorCommand(id, body.ColorName), cancellationToken);
        return TypedResults.NoContent();
    }

    /// <summary>
    /// Deletes the color; a color that cars still have answers 409.
    /// </summary>
    private static async Task<Results<NoContent, ProblemHttpResult>> Delete(
        IQueryHandler<GetColorByIdQuery, Color> getColorById,
        ICommandHandler<DeleteColorCommand> deleteColor,
        int id,
        CancellationToken cancellationToken)
    {
        if (await getColorById.HandleAsync(new GetColorByIdQuery(id), cancellationToken) is null)
        {
            return RestResults.NotFound("Color", id);
        }

        await deleteColor.HandleAsync(new DeleteColorCommand(id), cancellationToken);
        return TypedResults.NoContent();
    }
}
