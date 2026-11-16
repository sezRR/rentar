using FluentValidation;
using Rentar.Modules.Companies.Contracts;
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
/// Stores a new customer of the Identity user <see cref="UserId"/> and answers it with its id set. Both company fields
/// are optional: <see cref="CompanyName"/> is free text, stored as <c>null</c> when blank, and <see cref="CompanyId"/>
/// makes the customer one of that company's, which needs the user to be its employee. A company customer without a
/// name of its own is stored with the company's name.
/// </summary>
/// <remarks>
/// Throws <see cref="BusinessRuleException"/> when the Identity module does not know the user or holds it as inactive,
/// or when the Companies module does not hold the user as an employee of <see cref="CompanyId"/>.
/// </remarks>
public sealed record CreateCustomerCommand(int UserId, string CompanyName = null, int? CompanyId = null);

public sealed class CreateCustomerCommandValidator : UseCaseValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.CompanyName).MinimumLength(5).When(c => !string.IsNullOrWhiteSpace(c.CompanyName));
    }
}

public sealed class CreateCustomerCommandHandler(
    ICustomerDal customerDal,
    IIdentityModuleApi identityModule,
    ICompaniesModuleApi companiesModule) : ICommandHandler<CreateCustomerCommand, Customer>
{
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

        var companyName = string.IsNullOrWhiteSpace(command.CompanyName) ? null : command.CompanyName;
        if (command.CompanyId is { } companyId)
        {
            var employee = await companiesModule.GetEmployeeAsync(companyId, command.UserId, cancellationToken)
                ?? throw new BusinessRuleException(Messages.UserNotEmployeeOfCompany);

            companyName ??= employee.CompanyName;
        }

        var customer = new Customer { UserId = command.UserId, CompanyName = companyName, CompanyId = command.CompanyId };
        await customerDal.AddAsync(customer, cancellationToken);
        return customer;
    }
}
