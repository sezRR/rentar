using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;

namespace Rentar.Modules.Fleet.Application.Colors.Queries;

/// <summary>Answers the color, or <c>null</c> when there is none with that id.</summary>
public sealed record GetColorByIdQuery(int Id);

public sealed class GetColorByIdQueryHandler(IColorDal colorDal) : IQueryHandler<GetColorByIdQuery, Color>
{
    public Color Handle(GetColorByIdQuery query)
    {
        return colorDal.Get(c => c.Id == query.Id);
    }
}
