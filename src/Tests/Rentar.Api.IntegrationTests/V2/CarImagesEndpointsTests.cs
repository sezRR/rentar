using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Domain;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

public class CarImagesEndpointsTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task Upload_AnswersCreated_WithTheImageUnderItsCar()
    {
        var car = await CreateCarAsync();

        var response = await UploadImageAsync(car.Id, "car.png", TestImages.Png);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var image = await response.ReadAsAsync<CarImageBody>();
        Assert.Equal(car.Id, image.CarId);
        Assert.NotEqual(IImageStorage.DefaultImageName, image.ImagePath);
        Assert.Equal($"/api/v2/cars/{car.Id}/images/{image.Id}", response.Headers.Location?.OriginalString);
        Assert.True(File.Exists(UploadedFile(image.ImagePath)));

        var fetched = await Client.GetJsonAsync(response.Headers.Location!.OriginalString);
        Assert.Equal(image.ImagePath, (await fetched.ReadAsAsync<CarImageBody>()).ImagePath);

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task Upload_WithoutFile_GivesTheCarTheDefaultImage()
    {
        var car = await CreateCarAsync();
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("no file"), "note");

        var response = await Client.PostAsync($"/api/v2/cars/{car.Id}/images", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.Equal(IImageStorage.DefaultImageName, (await response.ReadAsAsync<CarImageBody>()).ImagePath);
    }

    [Fact]
    public async Task Upload_ForACarThatIsNotStored_IsNotFound()
    {
        var response = await UploadImageAsync(404, "car.png", TestImages.Png);

        await AssertProblemAsync(response, HttpStatusCode.NotFound, Messages.TheCarNotFound);
    }

    [Fact]
    public async Task UnknownImage_IsNotFound()
    {
        var car = await CreateCarAsync();

        var response = await Client.GetJsonAsync($"/api/v2/cars/{car.Id}/images/404");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, "Car image 404 was not found.");
    }

    [Fact]
    public async Task Upload_AllowsFiveImagesPerCar_AndRejectsTheSixth()
    {
        var car = await CreateCarAsync();

        for (var image = 1; image <= 5; image++)
        {
            Assert.Equal(HttpStatusCode.Created, (await UploadDefaultImageAsync(car.Id)).StatusCode);
        }

        var sixth = await UploadDefaultImageAsync(car.Id);

        await AssertProblemAsync(sixth, HttpStatusCode.BadRequest, DomainMessages.CarImageLimitReached);
        Assert.Equal(5, (await ImagesAsync(car.Id)).Count);
    }

    [Theory]
    [InlineData("car.png", nameof(TestImages.Png))]
    [InlineData("car.jpg", nameof(TestImages.Jpeg))]
    [InlineData("car.JPEG", nameof(TestImages.Jpeg))]
    public async Task Upload_AcceptsPngOrJpeg_WhateverTheExtensionCase(string fileName, string content)
    {
        var car = await CreateCarAsync();

        var response = await UploadImageAsync(car.Id, fileName, Bytes(content));

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var image = Assert.Single(await ImagesAsync(car.Id));
        Assert.NotEqual(IImageStorage.DefaultImageName, image.ImagePath);
        Assert.True(File.Exists(UploadedFile(image.ImagePath)));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Theory]
    [InlineData("notes.md", nameof(TestImages.Text))]
    [InlineData("car.gif", nameof(TestImages.Gif))]
    [InlineData("car", nameof(TestImages.Text))]
    [InlineData("fake.png", nameof(TestImages.Text))]
    [InlineData("fake.jpg", nameof(TestImages.Text))]
    [InlineData("car.png", nameof(TestImages.Jpeg))]
    public async Task Upload_OfAnythingButPngOrJpeg_IsRejected(string fileName, string content)
    {
        var car = await CreateCarAsync();

        var response = await UploadImageAsync(car.Id, fileName, Bytes(content));

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CarImageInvalidFile);
        Assert.Empty(await ImagesAsync(car.Id));
    }

    [Fact]
    public async Task Upload_WhenTheImageCanNotBeStored_RemovesItsFileAgain()
    {
        var car = await CreateCarAsync();
        var filesBefore = UploadedFiles();

        HttpResponseMessage response;
        await using (await Factory.RefuseAsync("fleet", "CarImages", "INSERT"))
        {
            response = await UploadImageAsync(car.Id, "car.png", TestImages.Png);
        }

        // The file was written before the row failed; the rolled back transaction takes it away again.
        await AssertProblemAsync(response, HttpStatusCode.InternalServerError);
        Assert.Empty(await ImagesAsync(car.Id));
        Assert.Equal(filesBefore, UploadedFiles());
    }

    [Fact]
    public async Task Replace_SwapsTheStoredFile()
    {
        var car = await CreateCarAsync();
        var original = await (await UploadImageAsync(car.Id, "car.png", TestImages.Png)).ReadAsAsync<CarImageBody>();

        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(TestImages.Jpeg), "file", "new.jpg");
        var response = await Client.PutAsync($"/api/v2/cars/{car.Id}/images/{original.Id}", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var replaced = Assert.Single(await ImagesAsync(car.Id));
        Assert.Equal(original.Id, replaced.Id);
        Assert.False(File.Exists(UploadedFile(original.ImagePath)));
        Assert.True(File.Exists(UploadedFile(replaced.ImagePath)));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task Replace_OfTheDefaultImage_KeepsTheDefaultFile()
    {
        var car = await CreateCarAsync();
        var original = await (await UploadDefaultImageAsync(car.Id)).ReadAsAsync<CarImageBody>();
        Assert.Equal(IImageStorage.DefaultImageName, original.ImagePath);

        using var form = new MultipartFormDataContent();
        form.Add(new ByteArrayContent(TestImages.Png), "file", "new.png");
        var response = await Client.PutAsync($"/api/v2/cars/{car.Id}/images/{original.Id}", form, TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.NotEqual(IImageStorage.DefaultImageName, Assert.Single(await ImagesAsync(car.Id)).ImagePath);
        Assert.True(File.Exists(UploadedFile(IImageStorage.DefaultImageName)));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task Replace_WithoutFile_IsRejected()
    {
        var car = await CreateCarAsync();
        var image = await (await UploadImageAsync(car.Id, "car.png", TestImages.Png)).ReadAsAsync<CarImageBody>();

        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("no file"), "note");
        var response = await Client.PutAsync($"/api/v2/cars/{car.Id}/images/{image.Id}", form, TestContext.Current.CancellationToken);

        await AssertProblemAsync(response, HttpStatusCode.BadRequest, Messages.CarImageInvalidFile);
        Assert.True(File.Exists(UploadedFile(image.ImagePath)));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task Delete_RemovesOnlyThatImageAndItsFile()
    {
        var car = await CreateCarAsync();
        var removed = await (await UploadImageAsync(car.Id, "first.png", TestImages.Png)).ReadAsAsync<CarImageBody>();
        var kept = await (await UploadImageAsync(car.Id, "second.jpg", TestImages.Jpeg)).ReadAsAsync<CarImageBody>();

        var response = await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images/{removed.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Equal(kept.Id, Assert.Single(await ImagesAsync(car.Id)).Id);
        Assert.False(File.Exists(UploadedFile(removed.ImagePath)));
        Assert.True(File.Exists(UploadedFile(kept.ImagePath)));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task Image_IsNotFoundUnderAnotherCar()
    {
        var car = await CreateCarAsync();
        var otherCar = await CreateCarAsync("Fiat", "White");
        var image = await (await UploadImageAsync(car.Id, "car.png", TestImages.Png)).ReadAsAsync<CarImageBody>();

        var response = await Client.GetJsonAsync($"/api/v2/cars/{otherCar.Id}/images/{image.Id}");
        var delete = await Client.DeleteResourceAsync($"/api/v2/cars/{otherCar.Id}/images/{image.Id}");

        await AssertProblemAsync(response, HttpStatusCode.NotFound, $"Car image {image.Id} was not found.");
        await AssertProblemAsync(delete, HttpStatusCode.NotFound);
        Assert.Single(await ImagesAsync(car.Id));

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
    }

    [Fact]
    public async Task DeleteAll_RemovesEveryImageOfTheCar_AndAlsoSucceedsWhenThereAreNone()
    {
        var car = await CreateCarAsync();
        var stored = UploadedFile((await (await UploadImageAsync(car.Id, "car.png", TestImages.Png)).ReadAsAsync<CarImageBody>()).ImagePath);
        await UploadImageAsync(car.Id, "car.jpg", TestImages.Jpeg);

        var first = await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
        var second = await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");

        Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, second.StatusCode);
        Assert.Empty(await ImagesAsync(car.Id));
        Assert.False(File.Exists(stored));
    }

    [Fact]
    public async Task DeleteAll_OfTheDefaultImage_KeepsTheDefaultFile()
    {
        var car = await CreateCarAsync();
        Assert.Equal(IImageStorage.DefaultImageName, (await (await UploadDefaultImageAsync(car.Id)).ReadAsAsync<CarImageBody>()).ImagePath);

        var response = await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        Assert.Empty(await ImagesAsync(car.Id));
        Assert.True(File.Exists(UploadedFile(IImageStorage.DefaultImageName)));
    }

    [Fact]
    public async Task CarImages_ListsTheImagesOfEveryCar()
    {
        var car = await CreateCarAsync();
        var otherCar = await CreateCarAsync("Fiat", "White");
        await UploadImageAsync(car.Id, "car.png", TestImages.Png);
        await UploadImageAsync(otherCar.Id, "car.jpg", TestImages.Jpeg);

        var response = await Client.GetJsonAsync("/api/v2/car-images");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var images = await response.ReadItemsAsync<CarImageBody>();
        Assert.Equal([car.Id, otherCar.Id], images.Select(i => i.CarId).Order());

        await Client.DeleteResourceAsync($"/api/v2/cars/{car.Id}/images");
        await Client.DeleteResourceAsync($"/api/v2/cars/{otherCar.Id}/images");
    }

    /// <summary>Uploads without a file, so the car gets the default image and nothing is written to disk.</summary>
    private async Task<HttpResponseMessage> UploadDefaultImageAsync(int carId)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent("no file"), "note");
        return await Client.PostAsync($"/api/v2/cars/{carId}/images", form, TestContext.Current.CancellationToken);
    }

    private async Task<List<CarImageBody>> ImagesAsync(int carId)
    {
        var response = await Client.GetJsonAsync($"/api/v2/cars/{carId}/images");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadItemsAsync<CarImageBody>();
    }

    /// <summary>Every file in the uploads folder, default image included, in name order.</summary>
    private string[] UploadedFiles()
    {
        return Directory.GetFiles(Path.GetDirectoryName(UploadedFile(IImageStorage.DefaultImageName))!).Order().ToArray();
    }

    /// <summary>The bytes a theory row names: one of <see cref="TestImages"/>.</summary>
    private static byte[] Bytes(string content)
    {
        return content switch
        {
            nameof(TestImages.Png) => TestImages.Png,
            nameof(TestImages.Jpeg) => TestImages.Jpeg,
            nameof(TestImages.Gif) => TestImages.Gif,
            _ => TestImages.Text
        };
    }
}
