using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.Data;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Common.Interfaces;

public interface ICompanyDal : IEntityRepository<Company>
{
    /// <summary>
    /// Stores a new company together with its roles, its employees and the roles granted to them, in one save.
    /// </summary>
    Task RegisterAsync(Company company, CancellationToken cancellationToken = default);
}
