using Rentar.Modules.Fleet.Domain;
using System;
using Xunit;

namespace Rentar.Domain.UnitTests.Fleet;

public class CarImageTests
{
    [Fact]
    public void WithFile_IsTheSameImageOfTheSameCar_WithTheNewFile()
    {
        var car = new Car(brandId: 1, colorId: 2, modelYear: 2024, dailyPrice: 450m, "Corolla 1.6", minimumFindeksRating: 0) { Id = 7 };
        var image = car.NewImage("old.jpg", new DateTime(2026, 9, 18, 10, 0, 0, DateTimeKind.Unspecified));
        var uploaded = new DateTime(2026, 9, 19, 10, 0, 0, DateTimeKind.Unspecified);

        var replaced = image.WithFile("new.jpg", uploaded);

        Assert.Equal(image.Id, replaced.Id);
        Assert.Equal(7, replaced.CarId);
        Assert.Equal("new.jpg", replaced.ImagePath);
        Assert.Equal(uploaded, replaced.Date);
        Assert.Equal("old.jpg", image.ImagePath);
    }
}
