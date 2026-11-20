using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Commands;

public sealed record UpdateBrandCommand(int Id, string BrandName);

public sealed class UpdateBrandCommandValidator : UseCaseValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidator()
    {
        RuleFor(b => b.BrandName).ValidBrandName();
    }
}

public sealed class UpdateBrandCommandHandler(IBrandDal brandDal) : ICommandHandler<UpdateBrandCommand>
{
    public Task HandleAsync(UpdateBrandCommand command, CancellationToken cancellationToken = default)
    {
        return brandDal.UpdateAsync(new Brand(command.BrandName) { Id = command.Id }, cancellationToken);
    }
}
