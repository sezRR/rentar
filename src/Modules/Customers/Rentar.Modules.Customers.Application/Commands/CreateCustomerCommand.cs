using FluentValidation;
using Rentar.Modules.Customers.Application.Common.Interfaces;
using Rentar.Modules.Customers.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Customers.Application.Commands;

/// <summary>Stores a new customer of the Identity user <see cref="UserId"/> and answers it with its id set.</summary>
public sealed record CreateCustomerCommand(int UserId, string CompanyName);

public sealed class CreateCustomerCommandValidator : UseCaseValidator<CreateCustomerCommand>
{
    public CreateCustomerCommandValidator()
    {
        RuleFor(c => c.CompanyName).NotEmpty().MinimumLength(5);
    }
}

public sealed class CreateCustomerCommandHandler(ICustomerDal customerDal) : ICommandHandler<CreateCustomerCommand, Customer>
{
    public async Task<Customer> HandleAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default)
    {
        var customer = new Customer { UserId = command.UserId, CompanyName = command.CompanyName };
        await customerDal.AddAsync(customer, cancellationToken);
        return customer;
    }
}
