using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Colors.Queries;

/// <summary>Answers the color, or <c>null</c> when there is none with that id.</summary>
public sealed record GetColorByIdQuery(int Id);

public sealed class GetColorByIdQueryHandler(IColorDal colorDal) : IQueryHandler<GetColorByIdQuery, Color>
{
    public Task<Color> HandleAsync(GetColorByIdQuery query, CancellationToken cancellationToken = default)
    {
        return colorDal.GetAsync(c => c.Id == query.Id, cancellationToken);
    }
}
