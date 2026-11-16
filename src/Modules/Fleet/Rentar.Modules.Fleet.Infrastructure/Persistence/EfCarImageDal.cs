using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Fleet.Infrastructure.Persistence;

public class EfCarImageDal(IDbContextFactory<FleetDbContext> contextFactory) : EfEntityRepositoryBase<CarImage, FleetDbContext>(contextFactory), ICarImageDal
{
}
