using Rentar.Modules.Rentals.Application.Common.Interfaces;
using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Rentals.Application.Queries;

/// <summary>Answers the rental, or <c>null</c> when there is none with that id.</summary>
public sealed record GetRentalByIdQuery(int Id);

public sealed class GetRentalByIdQueryHandler(IRentalDal rentalDal) : IQueryHandler<GetRentalByIdQuery, Rental>
{
    public Task<Rental> HandleAsync(GetRentalByIdQuery query, CancellationToken cancellationToken = default)
    {
        return rentalDal.GetAsync(r => r.Id == query.Id, cancellationToken);
    }
}
