using Rentar.Modules.Rentals.Domain;
using Rentar.Shared.Application.Data;

namespace Rentar.Modules.Rentals.Application.Common.Interfaces;

public interface IRentalDal : IEntityRepository<Rental>
{
}
