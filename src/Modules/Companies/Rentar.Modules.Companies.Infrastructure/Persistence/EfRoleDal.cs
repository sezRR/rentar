using Microsoft.EntityFrameworkCore;
using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Infrastructure.Persistence;

namespace Rentar.Modules.Companies.Infrastructure.Persistence;

public class EfRoleDal(IDbContextFactory<CompaniesDbContext> contextFactory)
    : EfEntityRepositoryBase<Role, CompaniesDbContext>(contextFactory), IRoleDal
{
}
