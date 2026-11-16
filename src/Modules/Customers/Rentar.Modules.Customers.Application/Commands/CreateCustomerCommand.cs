using FluentValidation;
using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Commands;

/// <summary>
/// Stores a new customer of the Identity user <see cref="UserId"/> and answers it with its id set; throws
/// <see cref="BusinessRuleException"/> when the Identity module does not know that user or holds it as inactive.
/// </summary>
public sealed record CreateCustomerCommand(int UserId, string CompanyName);

public sealed class CreateCustomerCommandValidator : UseCaseValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.CompanyName).NotEmpty().MinimumLength(5);
    }
}

public sealed class CreateCustomerCommandHandler(ICustomerDal customerDal, IIdentityModuleApi identityModule)
    : ICommandHandler<CreateCustomerCommand, Customer>
{
    public async Task<Customer> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        // The user lives in another module, so no foreign key guards the reference; that module is asked instead.
        var user = (await identityModule.GetUsersAsync([command.UserId], cancellationToken)).SingleOrDefault();
        if (user is null)
        {
            throw new BusinessRuleException(Messages.UserNotFound);
        }

        if (!user.IsActive)
        {
            throw new BusinessRuleException(Messages.UserNotActive);
        }

        var customer = new Customer { UserId = command.UserId, CompanyName = command.CompanyName };
        await customerDal.AddAsync(customer, cancellationToken);
        return customer;
    }
}
