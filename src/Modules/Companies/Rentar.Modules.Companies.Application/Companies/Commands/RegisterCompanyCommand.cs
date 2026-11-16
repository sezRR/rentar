using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Application.Roles;
using Rentar.Modules.Companies.Domain;
using Rentar.Modules.Identity.Contracts;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Companies.Commands;

/// <summary>
/// Registers a company with the predefined roles, and makes the Identity user <see cref="UserId"/> its first employee
/// with the Owner role. Answers the stored company. Throws <see cref="BusinessRuleException"/> when Identity does not
/// know the user or holds it as inactive.
/// </summary>
public sealed record RegisterCompanyCommand(int UserId, string Name);

public sealed class RegisterCompanyCommandValidator : UseCaseValidator<RegisterCompanyCommand>
{
    public RegisterCompanyCommandValidator() => RuleFor(c => c.Name).ValidCompanyName();
}

public sealed class RegisterCompanyCommandHandler(ICompanyDal companyDal, IIdentityModuleApi identityModule)
    : ICommandHandler<RegisterCompanyCommand, Company>
{
    public async Task<Company> HandleAsync(RegisterCompanyCommand command, CancellationToken cancellationToken = default)
    {
        await identityModule.RequireActiveUserAsync(command.UserId, cancellationToken);

        var roles = PredefinedRoles.All.Select(role => role.ToRole()).ToList();
        var owner = new Employee
        {
            UserId = command.UserId,
            Roles = [roles.Single(role => role.Name == PredefinedRoles.Owner.Name)]
        };

        var company = new Company { Name = command.Name, Roles = roles, Employees = [owner] };
        await companyDal.RegisterAsync(company, cancellationToken);
        return company;
    }
}
