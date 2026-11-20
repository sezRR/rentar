using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Commands;

/// <summary>Replaces the car; throws <see cref="BusinessRuleException"/> when its brand or color is not stored.</summary>
public sealed record UpdateCarCommand(
    int Id,
    int BrandId,
    int ColorId,
    int ModelYear,
    decimal DailyPrice,
    string Description,
    int MinimumFindeksRating);

public sealed class UpdateCarCommandValidator : UseCaseValidator<UpdateCarCommand>
{
    public UpdateCarCommandValidator()
    {
        RuleFor(c => c.Description).ValidDescription();
        RuleFor(c => c.ModelYear).ValidModelYear();
        RuleFor(c => c.DailyPrice).ValidDailyPrice();
    }
}

public sealed class UpdateCarCommandHandler(ICarDal carDal, IBrandDal brandDal, IColorDal colorDal) : ICommandHandler<UpdateCarCommand>
{
    public async Task HandleAsync(UpdateCarCommand command, CancellationToken cancellationToken = default)
    {
        await CarReferences.EnsureExistAsync(brandDal, colorDal, command.BrandId, command.ColorId, cancellationToken);

        var car = new Car(
            command.BrandId,
            command.ColorId,
            command.ModelYear,
            command.DailyPrice,
            command.Description,
            command.MinimumFindeksRating)
        {
            Id = command.Id
        };

        await carDal.UpdateAsync(car, cancellationToken);
    }
}
