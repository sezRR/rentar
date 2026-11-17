using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Customers.Contracts.Events;
using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Application.Aspects.Transaction;
using Rentar.Shared.Application.Events;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Commands;

/// <summary>
/// Stores the customer account of the Identity user <see cref="UserId"/>, publishes
/// <see cref="CustomerCreatedEvent"/> (on which Findeks gives it a score) and answers it with its id set.
/// <see cref="CompanyId"/> makes it one of that company's customers, which needs the user to be its employee; leave it
/// out for a customer with no partner company.
/// </summary>
/// <remarks>
/// Throws <see cref="BusinessRuleException"/> when the Identity module does not know the user or holds it as inactive,
/// or when the Companies module does not hold the user as an employee of <see cref="CompanyId"/>, and
/// <see cref="ConflictException"/> when the user already has a customer account.
/// </remarks>
public sealed record CreateCustomerCommand(int UserId, int? CompanyId = null);

public sealed class CreateCustomerCommandHandler(
    ICustomerDal customerDal,
    IIdentityModuleApi identityModule,
    ICompaniesModuleApi companiesModule,
    IEventPublisher eventPublisher) : ICommandHandler<CreateCustomerCommand, Customer>
{
    [TransactionScopeAspect]
    public async Task<Customer> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        // The user and the company live in other modules, so no foreign key guards the references; those modules are
        // asked instead.
        var user = (await identityModule.GetUsersAsync([command.UserId], cancellationToken)).SingleOrDefault();
        if (user is null)
        {
            throw new BusinessRuleException(Messages.UserNotFound);
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(Messages.UserNotActive);
        }

        // The unique index refuses it too; asking first turns that into an answer the caller can act on.
        if (await customerDal.AnyAsync(c => c.UserId == command.UserId, cancellationToken))
        {
            throw new ConflictException(Messages.UserAlreadyCustomer);
        }

        if (command.CompanyId is { } companyId
            && await companiesModule.GetEmployeeAsync(companyId, command.UserId, cancellationToken) is null)
        {
            throw new BusinessRuleException(Messages.UserNotEmployeeOfCompany);
        }

        var customer = new Customer { UserId = command.UserId, CompanyId = command.CompanyId };
        await customerDal.AddAsync(customer, cancellationToken);

        await eventPublisher.PublishAsync(new CustomerCreatedEvent(customer.Id, customer.UserId, customer.CompanyId), cancellationToken);
        return customer;
    }
}
