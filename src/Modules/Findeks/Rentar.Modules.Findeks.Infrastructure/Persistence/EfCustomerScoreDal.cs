using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Findeks.Application.Common.Interfaces;
using Rentar.Modules.Findeks.Domain;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Findeks.Infrastructure.Persistence;

public class EfCustomerScoreDal(IDbContextFactory<FindeksDbContext> contextFactory)
    : EfEntityRepositoryBase<CustomerScore, FindeksDbContext>(contextFactory), ICustomerScoreDal
{
}
