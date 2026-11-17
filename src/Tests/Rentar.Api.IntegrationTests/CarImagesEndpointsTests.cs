using Microsoft.Extensions.DependencyInjection;
using Rentar.Modules.Fleet.Application;
using Rentar.Modules.Fleet.Domain;
using Rentar.Modules.Fleet.Infrastructure.Storage;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Rentar.Modules.Fleet.Application.Common.Interfaces;
using Xunit;

namespace Rentar.Api.IntegrationTests;

public class CarImagesEndpointsTests(RentarApiFactory factory) : IntegrationTestBase(factory)
{
    private static readonly byte[] PngBytes = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00 };
    private static readonly byte[] JpegBytes = { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
    private static readonly byte[] GifBytes = Encoding.ASCII.GetBytes("GIF89a....");
    private static readonly byte[] TextBytes = Encoding.UTF8.GetBytes("# not an image");

    [Theory]
    [InlineData("car.png")]
    [InlineData("car.jpg")]
    [InlineData("car.JPEG")]
    public async Task Add_WithPngOrJpeg_StoresImage(string fileName)
    {
        var bytes = fileName.EndsWith(".png") ? PngBytes : JpegBytes;
        var carId = await AddCarAsync();

        var response = await UploadAsync(carId, fileName, bytes);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var images = await GetImagesAsync(carId);
        var image = Assert.Single(images);
        Assert.NotEqual("default.png", image.ImagePath);

        // Removes the stored file again so the test leaves wwwroot/uploads untouched.
        var delete = await DeleteImagesAsync(carId);
        Assert.Equal(HttpStatusCode.OK, delete.StatusCode);
    }

    [Theory]
    [InlineData("notes.md")]
    [InlineData("car.gif")]
    [InlineData("car")]
    public async Task Add_WithUnsupportedExtension_IsRejected(string fileName)
    {
        var bytes = fileName.EndsWith(".gif") ? GifBytes : TextBytes;
        var carId = await AddCarAsync();

        var response = await UploadAsync(carId, fileName, bytes);

        await AssertRejectedAsync(response);
        Assert.Empty(await GetImagesAsync(carId));
    }

    [Theory]
    [InlineData("fake.png")]
    [InlineData("fake.jpg")]
    public async Task Add_WithImageExtensionButOtherContent_IsRejected(string fileName)
    {
        var carId = await AddCarAsync();

        var response = await UploadAsync(carId, fileName, TextBytes);

        await AssertRejectedAsync(response);
        Assert.Empty(await GetImagesAsync(carId));
    }

    [Fact]
    public async Task Add_WithJpegContentNamedPng_IsRejected()
    {
        var carId = await AddCarAsync();

        var response = await UploadAsync(carId, "car.png", JpegBytes);

        await AssertRejectedAsync(response);
    }

    [Fact]
    public async Task Delete_RemovesUploadedFileFromDisk()
    {
        var carId = await AddCarAsync();
        await UploadAsync(carId, "car.png", PngBytes);
        var stored = UploadedFile(Assert.Single(await GetImagesAsync(carId)).ImagePath);
        Assert.True(File.Exists(stored));

        var response = await DeleteImagesAsync(carId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(File.Exists(stored));
        Assert.Empty(await GetImagesAsync(carId));
    }

    [Fact]
    public async Task Delete_OfDefaultImage_KeepsDefaultFile()
    {
        var carId = await AddCarAsync();
        await UploadWithoutFileAsync(carId);
        Assert.Equal(IImageStorage.DefaultImageName, Assert.Single(await GetImagesAsync(carId)).ImagePath);

        var response = await DeleteImagesAsync(carId);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(File.Exists(UploadedFile(IImageStorage.DefaultImageName)));
        Assert.Empty(await GetImagesAsync(carId));
    }

    [Fact]
    public async Task Update_RemovesReplacedFileFromDisk()
    {
        var carId = await AddCarAsync();
        await UploadAsync(carId, "car.png", PngBytes);
        var original = Assert.Single(await GetImagesAsync(carId));

        var response = await UpdateAsync(original, "new.jpg", JpegBytes);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updated = Assert.Single(await GetImagesAsync(carId));
        Assert.False(File.Exists(UploadedFile(original.ImagePath)));
        Assert.True(File.Exists(UploadedFile(updated.ImagePath)));

        await DeleteImagesAsync(carId);
    }

    [Fact]
    public async Task Update_OfDefaultImage_KeepsDefaultFile()
    {
        var carId = await AddCarAsync();
        await UploadWithoutFileAsync(carId);
        var original = Assert.Single(await GetImagesAsync(carId));

        var response = await UpdateAsync(original, "new.png", PngBytes);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.True(File.Exists(UploadedFile(IImageStorage.DefaultImageName)));
        Assert.NotEqual(IImageStorage.DefaultImageName, Assert.Single(await GetImagesAsync(carId)).ImagePath);

        await DeleteImagesAsync(carId);
    }

    [Fact]
    public async Task DeletingCar_RemovesItsImagesAndFiles()
    {
        var carId = await AddCarAsync();
        await UploadAsync(carId, "car.png", PngBytes);
        await UploadWithoutFileAsync(carId);
        var images = await GetImagesAsync(carId);
        var stored = UploadedFile(images.Single(i => i.ImagePath != IImageStorage.DefaultImageName).ImagePath);

        var cars = await Client.GetJsonAsync("/api/v1/cars/getall");
        var car = (await cars.ReadAsAsync<V1Response<List<CarBody>>>()).Data.Single();
        var response = await Client.PostJsonAsync("/api/v1/cars/delete", car);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await GetImagesAsync(carId));
        Assert.False(File.Exists(stored));
        Assert.True(File.Exists(UploadedFile(IImageStorage.DefaultImageName)));
    }

    [Fact]
    public async Task DeletingCar_WhenCarDeleteFails_KeepsItsImagesAndFiles()
    {
        var carId = await AddCarAsync();
        await UploadAsync(carId, "car.png", PngBytes);
        var stored = UploadedFile(Assert.Single(await GetImagesAsync(carId)).ImagePath);

        HttpResponseMessage response;
        await using (await Factory.RefuseAsync("fleet", "Cars", "DELETE"))
        {
            response = await Client.PostJsonAsync("/api/v1/cars/delete", new CarBody { Id = carId });
        }

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Single(await GetImagesAsync(carId));
        Assert.True(File.Exists(stored));

        await DeleteImagesAsync(carId);
        Assert.False(File.Exists(stored));
    }

    [Fact]
    public async Task Add_WhenStoringTheImageFails_RemovesItsFileAgain()
    {
        var carId = await AddCarAsync();
        var filesBefore = UploadedFiles();

        HttpResponseMessage response;
        await using (await Factory.RefuseAsync("fleet", "CarImages", "INSERT"))
        {
            response = await UploadAsync(carId, "car.png", PngBytes);
        }

        // The file was written before the row failed; the rolled back transaction takes it away again.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(await GetImagesAsync(carId));
        Assert.Equal(filesBefore, UploadedFiles());
    }

    [Fact]
    public async Task Add_ForACarThatIsNotStored_IsRejected()
    {
        var response = await UploadAsync(999, "car.png", PngBytes);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(Messages.TheCarNotFound, (await response.ReadAsAsync<V1Response>()).Message);
        Assert.Empty(await GetImagesAsync(999));
    }

    private string[] UploadedFiles()
    {
        return Directory.GetFiles(Path.GetDirectoryName(UploadedFile(IImageStorage.DefaultImageName))!).Order().ToArray();
    }

    private string UploadedFile(string fileName)
    {
        var storage = (LocalImageStorage)Factory.Services.GetRequiredService<IImageStorage>();
        return Path.Combine(storage.UploadsPath, fileName);
    }

    private Task<HttpResponseMessage> DeleteImagesAsync(int carId)
    {
        return Client.DeleteAsync($"/api/v1/carimages/delete?id={carId}", TestContext.Current.CancellationToken);
    }

    private async Task UploadWithoutFileAsync(int carId)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(carId.ToString()), "carId");

        await Client.PostAsync("/api/v1/carimages/add", form, TestContext.Current.CancellationToken);
    }

    private async Task<HttpResponseMessage> UpdateAsync(CarImageBody carImage, string fileName, byte[] bytes)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(carImage.Id.ToString()), "Id");
        form.Add(new StringContent(carImage.CarId.ToString()), "CarId");
        form.Add(new ByteArrayContent(bytes), "objectFile", fileName);

        return await Client.PostAsync("/api/v1/carimages/update", form, TestContext.Current.CancellationToken);
    }

    private static async Task AssertRejectedAsync(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = await response.ReadAsAsync<V1Response>();
        Assert.Equal(Messages.CarImageInvalidFile, result.Message);
    }

    private async Task<HttpResponseMessage> UploadAsync(int carId, string fileName, byte[] bytes)
    {
        using var form = new MultipartFormDataContent();
        form.Add(new StringContent(carId.ToString()), "carId");
        form.Add(new ByteArrayContent(bytes), "file", fileName);

        return await Client.PostAsync("/api/v1/carimages/add", form, TestContext.Current.CancellationToken);
    }

    private async Task<List<CarImageBody>> GetImagesAsync(int carId)
    {
        var response = await Client.GetJsonAsync($"/api/v1/carimages/getbycarid?carId={carId}");
        var result = await response.ReadAsAsync<V1Response<List<CarImageBody>>>();
        return result.Data;
    }

    private async Task<int> AddCarAsync()
    {
        await Client.PostJsonAsync("/api/v1/brands/add", new BrandBody { BrandName = "Toyota" });
        await Client.PostJsonAsync("/api/v1/colors/add", new ColorBody { ColorName = "Red" });

        var car = new CarBody
        {
            BrandId = 1,
            ColorId = 1,
            ModelYear = 2021,
            DailyPrice = 950m,
            Description = "Corolla 1.6",
            MinimumFindeksRating = 700
        };
        await Client.PostJsonAsync("/api/v1/cars/add", car);

        var cars = await Client.GetJsonAsync("/api/v1/cars/getall");
        var listed = await cars.ReadAsAsync<V1Response<List<CarBody>>>();
        return listed.Data.Single().Id;
    }
}
