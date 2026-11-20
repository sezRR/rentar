using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Brands.Commands;

/// <summary>Stores a new brand and answers it with its id set.</summary>
public sealed record CreateBrandCommand(string BrandName);

public sealed class CreateBrandCommandValidator : UseCaseValidator<CreateBrandCommand>
{
    public CreateBrandCommandValidator()
    {
        RuleFor(b => b.BrandName).ValidBrandName();
    }
}

public sealed class CreateBrandCommandHandler(IBrandDal brandDal) : ICommandHandler<CreateBrandCommand, Brand>
{
    public async Task<Brand> HandleAsync(CreateBrandCommand command, CancellationToken cancellationToken = default)
    {
        var brand = new Brand(command.BrandName);
        await brandDal.AddAsync(brand, cancellationToken);
        return brand;
    }
}
