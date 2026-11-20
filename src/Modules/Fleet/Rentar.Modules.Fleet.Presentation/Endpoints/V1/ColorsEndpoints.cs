using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Application.Colors.Commands;
using Rentar.Modules.Fleet.Application.Colors.Queries;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Paging;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Presentation.Endpoints.V1;

public class ColorsEndpoints : IEndpoint
{
    public void MapEndpoints(IEndpointRouteBuilder builder)
    {
        var group = builder.MapVersionedGroup("Colors", "colors", 1.0);

        group.MapGet("/getall", GetAll);
        group.MapGet("/getbyid", GetById);
        group.MapPost("/add", Add);
        group.MapPost("/update", Update);
        group.MapPost("/delete", Delete);
    }

    private static async Task<Ok<V1Response<List<Color>>>> GetAll(IQueryHandler<GetColorsQuery, Page<Color>> getColors, CancellationToken cancellationToken)
    {
        return V1Results.Ok((await getColors.HandleAsync(new GetColorsQuery(), cancellationToken)).Items, Messages.ColorsListed);
    }

    private static async Task<Ok<V1Response<Color>>> GetById(IQueryHandler<GetColorByIdQuery, Color> getColorById, int id, CancellationToken cancellationToken)
    {
        return V1Results.Ok(await getColorById.HandleAsync(new GetColorByIdQuery(id), cancellationToken), null);
    }

    private static async Task<Ok<V1Response>> Add(ICommandHandler<CreateColorCommand, Color> createColor, CreateColorCommand command, CancellationToken cancellationToken)
    {
        await createColor.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.ColorAdded);
    }

    private static async Task<Ok<V1Response>> Update(ICommandHandler<UpdateColorCommand> updateColor, UpdateColorCommand command, CancellationToken cancellationToken)
    {
        await updateColor.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.ColorUpdated);
    }

    private static async Task<Ok<V1Response>> Delete(ICommandHandler<DeleteColorCommand> deleteColor, DeleteColorCommand command, CancellationToken cancellationToken)
    {
        await deleteColor.HandleAsync(command, cancellationToken);
        return V1Results.Ok(Messages.ColorDeleted);
    }
}
