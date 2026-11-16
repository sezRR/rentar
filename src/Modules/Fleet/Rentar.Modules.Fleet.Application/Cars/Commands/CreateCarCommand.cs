using Rentar.Modules.Fleet.Application.Cars.Queries;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Rentar.Modules.Fleet.Domain;
using Rentar.Shared.Application.Aspects.Caching;
using Rentar.Shared.Application.UseCases;
using Rentar.Shared.Application.Validation;

namespace Rentar.Modules.Fleet.Application.Cars.Commands;

/// <summary>Stores a new car and answers it with its id set.</summary>
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

public sealed class CreateCarCommandHandler(ICarDal carDal) : ICommandHandler<CreateCarCommand, Car>
{
    /// <summary>
    /// Matches the cache keys of the car queries, e.g. <see cref="GetCarsByBrandQueryHandler"/>, so a new car shows up at once.
    /// </summary>
    private const string CarQueries = @"Rentar\.Modules\.Fleet\.Application\.Cars\.Queries\.";

    [CacheRemoveAspect(CarQueries)]
    public Car Handle(CreateCarCommand command)
    {
        var car = new Car
        {
            BrandId = command.BrandId,
            ColorId = command.ColorId,
            ModelYear = command.ModelYear,
            DailyPrice = command.DailyPrice,
            Description = command.Description,
            MinimumFindeksRating = command.MinimumFindeksRating
        };

        carDal.Add(car);
        return car;
    }
}
