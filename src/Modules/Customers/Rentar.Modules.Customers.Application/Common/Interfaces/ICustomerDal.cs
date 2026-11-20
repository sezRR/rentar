using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.Data;

namespace Rentar.Modules.Customers.Application.Common.Interfaces;

public interface ICustomerDal : IEntityRepository<Customer>
{
}
