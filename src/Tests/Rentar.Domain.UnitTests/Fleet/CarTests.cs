using Rentar.Modules.Fleet.Domain;
using System;
using Xunit;

namespace Rentar.Domain.UnitTests.Fleet;

public class CarTests
{
    private static readonly DateTime UploadDate = new(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified);

    [Fact]
    public void Constructor_KeepsWhatItWasGiven()
    {
        var car = new Car(brandId: 1, colorId: 2, modelYear: 2024, dailyPrice: 450m, description: "Corolla 1.6", minimumFindeksRating: 500);

        Assert.Equal(1, car.BrandId);
        Assert.Equal(2, car.ColorId);
        Assert.Equal(2024, car.ModelYear);
        Assert.Equal(450m, car.DailyPrice);
        Assert.Equal("Corolla 1.6", car.Description);
        Assert.Equal(500, car.MinimumFindeksRating);
        Assert.Empty(car.Images);
    }

    [Fact]
    public void Constructor_TakesTheShortestDescription()
    {
        var car = NewCar(description: new string('a', Car.DescriptionMinLength));

        Assert.Equal(Car.DescriptionMinLength, car.Description.Length);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("a")]
    public void Constructor_RefusesDescriptionThatIsBlankOrTooShort(string description)
    {
        Refused.With(DomainMessages.CarDescriptionTooShort, () => NewCar(description: description));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Constructor_RefusesModelYearThatIsNotPositive(int modelYear)
    {
        Refused.With(DomainMessages.CarModelYearInvalid, () => NewCar(modelYear: modelYear));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-450)]
    public void Constructor_RefusesDailyPriceThatIsNotAboveZero(double dailyPrice)
    {
        Refused.With(DomainMessages.CarDailyPriceInvalid, () => NewCar(dailyPrice: (decimal)dailyPrice));
    }

    [Fact]
    public void Constructor_TakesTheSmallestPositiveYearAndPrice()
    {
        var car = NewCar(modelYear: 1, dailyPrice: 0.01m);

        Assert.Equal(1, car.ModelYear);
        Assert.Equal(0.01m, car.DailyPrice);
    }

    [Fact]
    public void NewImage_IsOfTheCar_AndLeavesTheCarAsItIs()
    {
        var car = NewCar(id: 7);

        var image = car.NewImage("cars/7/front.jpg", UploadDate);

        Assert.Equal(7, image.CarId);
        Assert.Equal("cars/7/front.jpg", image.ImagePath);
        Assert.Equal(UploadDate, image.Date);
        Assert.Empty(car.Images);
    }

    [Fact]
    public void NewImage_TakesTheLastImageTheCarHasRoomFor()
    {
        var car = WithImages(Car.MaxImages - 1);

        var image = car.NewImage("last.jpg", UploadDate);

        Assert.Equal("last.jpg", image.ImagePath);
    }

    [Fact]
    public void NewImage_RefusesImageBeyondTheLimit()
    {
        var car = WithImages(Car.MaxImages);

        Refused.With(DomainMessages.CarImageLimitReached, () => car.NewImage("sixth.jpg", UploadDate));
    }

    [Fact]
    public void EnsureRoomForImage_RefusesCarAtTheLimit_AndPassesBelowIt()
    {
        WithImages(Car.MaxImages - 1).EnsureRoomForImage();

        Refused.With(DomainMessages.CarImageLimitReached, WithImages(Car.MaxImages).EnsureRoomForImage);
    }

    /// <summary>A car as read with its images, holding <paramref name="count"/> of them.</summary>
    private static Car WithImages(int count)
    {
        var car = NewCar(id: 7);

        for (var i = 0; i < count; i++)
        {
            car.Images.Add(car.NewImage($"cars/7/{i}.jpg", UploadDate));
        }

        return car;
    }

    private static Car NewCar(
        int id = 0,
        int modelYear = 2024,
        decimal dailyPrice = 450m,
        string description = "Corolla 1.6")
    {
        return new Car(brandId: 1, colorId: 2, modelYear, dailyPrice, description, minimumFindeksRating: 0) { Id = id };
    }
}
