using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Aspects.Caching;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;
using Rentar.Shared.Kernel.Exceptions;
using System.Threading;
using System.Threading.Tasks;

namespace Rentar.Modules.Fleet.Application.Cars.Commands;

/// <summary>
/// Stores a new car and answers it with its id set. Throws <see cref="BusinessRuleException"/> when its brand or
/// color is not stored.
/// </summary>
public sealed record CreateCarCommand(
    int BrandId,
    int ColorId,
    int ModelYear,
    decimal DailyPrice,
    string Description,
    int MinimumFindeksRating);

public sealed class CreateCarCommandValidator : UseCaseValidator<CreateCarCommand>
{
    public CreateCarCommandValidator()
    {
        RuleFor(c => c.Description).ValidDescription();
        RuleFor(c => c.ModelYear).ValidModelYear();
        RuleFor(c => c.DailyPrice).ValidDailyPrice();
    }
}

public sealed class CreateCarCommandHandler(ICarDal carDal, IBrandDal brandDal, IColorDal colorDal) : ICommandHandler<CreateCarCommand, Car>
{
    /// <summary>
    /// Matches the cache keys of the car queries, e.g. <see cref="GetCarsByBrandQueryHandler"/>, so a new car shows up at once.
    /// </summary>
    private const string CarQueries = @"Rentar\.Modules\.Fleet\.Application\.Cars\.Queries\.";

    [CacheRemoveAspect(CarQueries)]
    public async Task<Car> HandleAsync(CreateCarCommand command, CancellationToken cancellationToken = default)
    {
        await CarReferences.EnsureExistAsync(brandDal, colorDal, command.BrandId, command.ColorId, cancellationToken);

        var car = new Car(
            command.BrandId,
            command.ColorId,
            command.ModelYear,
            command.DailyPrice,
            command.Description,
            command.MinimumFindeksRating);

        await carDal.AddAsync(car, cancellationToken);
        return car;
    }
}
