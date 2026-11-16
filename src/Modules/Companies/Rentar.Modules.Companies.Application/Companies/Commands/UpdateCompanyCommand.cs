using Rentar.Modules.Companies.Application.Common.Interfaces;
using Rentar.Modules.Companies.Contracts;
using Rentar.Modules.Companies.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Companies.Application.Companies.Commands;

/// <summary>
/// Renames the company on behalf of its employee <see cref="ActorUserId"/>, who needs
/// <see cref="CompanyPermissions.CompanyManage"/>; throws <see cref="NotFoundException"/> or
/// <see cref="ForbiddenException"/> otherwise.
/// </summary>
public sealed record UpdateCompanyCommand(int ActorUserId, int CompanyId, string Name);

public sealed class UpdateCompanyCommandValidator : UseCaseValidator<UpdateCompanyCommand>
{
    public UpdateCompanyCommandValidator() => RuleFor(c => c.Name).ValidCompanyName();
}

public sealed class UpdateCompanyCommandHandler(ICompanyDal companyDal, IEmployeeDal employeeDal) : ICommandHandler<UpdateCompanyCommand>
{
    public async Task HandleAsync(UpdateCompanyCommand command, CancellationToken cancellationToken = default)
    {
        await employeeDal.RequirePermissionAsync(command.CompanyId, command.ActorUserId, CompanyPermissions.CompanyManage, cancellationToken);

        await companyDal.UpdateAsync(new Company { Id = command.CompanyId, Name = command.Name }, cancellationToken);
    }
}
