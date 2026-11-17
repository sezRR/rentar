using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Commands;

/// <summary>
/// The Identity user <see cref="UserId"/> no longer works at the Companies company <see cref="CompanyId"/>, so their
/// customer account stops renting for it and becomes one with no partner company. A customer of another company, or
/// none, stays as it is.
/// </summary>
public sealed record ClearCustomerCompanyCommand(int UserId, int CompanyId);

public sealed class ClearCustomerCompanyCommandHandler(ICustomerDal customerDal) : ICommandHandler<ClearCustomerCompanyCommand>
{
    public async Task HandleAsync(ClearCustomerCompanyCommand command, CancellationToken cancellationToken = default)
    {
        var customer = await customerDal.GetAsync(
            c => c.UserId == command.UserId && c.CompanyId == command.CompanyId,
            cancellationToken);

        if (customer is null)
        {
            return;
        }

        await customerDal.UpdateAsync(new Customer { Id = customer.Id, UserId = customer.UserId, CompanyId = null }, cancellationToken);
    }
}
