using Rentar.Modules.Fleet.Presentation.Endpoints.V2;
using Rentar.Modules.Identity.Presentation.Endpoints.V2;
using Rentar.Shared.Presentation.Endpoints;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Rentar.Api.IntegrationTests.V2;

/// <summary>
/// Every v2 list answers one page, <c>{ items, page: { hasMore, nextCursor } }</c>, and the next page is asked for
/// with that cursor.
/// </summary>
public class PagingTests(RentarApiFactory factory) : V2TestBase(factory)
{
    [Fact]
    public async Task List_AnswersTheFirst25_WithTheCursorOfTheLastOne()
    {
        await CreateBrandsAsync(30);

        var response = await Client.GetJsonAsync("/api/v2/brands");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.ReadAsAsync<PagedResponse<BrandBody>>();
        Assert.Equal(Enumerable.Range(1, 25), page.Items.Select(b => b.Id));
        Assert.Equal(new PageInfo(true, "eyJpZCI6MjV9"), page.Page);
    }

    [Fact]
    public async Task Cursor_IsBase64UrlJsonOfTheLastKey()
    {
        await CreateBrandsAsync(11);

        var page = await PageAsync<BrandBody>("/api/v2/brands?limit=10");

        // {"id":10}, base64url without padding.
        Assert.Equal("eyJpZCI6MTB9", page.Page.NextCursor);
    }

    [Fact]
    public async Task Cursor_ContinuesAfterTheLastItem_UpToTheLastPage()
    {
        await CreateBrandsAsync(30);
        var first = await PageAsync<BrandBody>("/api/v2/brands");

        var response = await Client.GetJsonAsync($"/api/v2/brands?cursor={first.Page.NextCursor}");

        Assert.Contains("\"page\":{\"hasMore\":false,\"nextCursor\":null}", await response.ReadTextAsync());
        var last = await response.ReadAsAsync<PagedResponse<BrandBody>>();
        Assert.Equal(Enumerable.Range(26, 5), last.Items.Select(b => b.Id));
        Assert.Equal(new PageInfo(false, null), last.Page);
    }

    [Fact]
    public async Task FollowingEveryCursor_ListsEachItemOnce_InOrder()
    {
        await CreateBrandsAsync(30);
        var ids = new List<int>();
        var pages = 0;

        var url = "/api/v2/brands?limit=7";
        while (url is not null)
        {
            var page = await PageAsync<BrandBody>(url);
            Assert.InRange(page.Items.Count, 1, 7);
            ids.AddRange(page.Items.Select(b => b.Id));
            pages++;
            url = page.Page.HasMore ? $"/api/v2/brands?limit=7&cursor={page.Page.NextCursor}" : null;
        }

        Assert.Equal(Enumerable.Range(1, 30), ids);
        Assert.Equal(5, pages);
    }

    [Fact]
    public async Task Cursor_IsNotThrownOff_ByItemsAddedOrRemovedBeforeIt()
    {
        await CreateBrandsAsync(25);
        var first = await PageAsync<BrandBody>("/api/v2/brands?limit=10");

        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteResourceAsync("/api/v2/brands/3")).StatusCode);
        await CreateBrandAsync("Newcomer");

        var second = await PageAsync<BrandBody>($"/api/v2/brands?limit=10&cursor={first.Page.NextCursor}");
        Assert.Equal(Enumerable.Range(11, 10), second.Items.Select(b => b.Id));
    }

    [Fact]
    public async Task CursorOfTheLastItem_AnswersAnEmptyLastPage()
    {
        await CreateBrandsAsync(3);

        // {"id":3}: a cursor a client could hold from before the list stopped growing.
        var page = await PageAsync<BrandBody>("/api/v2/brands?cursor=eyJpZCI6M30");

        Assert.Empty(page.Items);
        Assert.Equal(new PageInfo(false, null), page.Page);
    }

    [Fact]
    public async Task EmptyList_IsOneEmptyLastPage()
    {
        var page = await PageAsync<ColorBody>("/api/v2/colors");

        Assert.Empty(page.Items);
        Assert.Equal(new PageInfo(false, null), page.Page);
    }

    [Fact]
    public async Task FilteredList_ContinuesWithTheSameFilter()
    {
        var toyota = await CreateBrandAsync();
        var fiat = await CreateBrandAsync("Fiat");
        var red = await CreateColorAsync();
        await CreateCarAsync(toyota.Id, red.Id, "First Corolla");
        await CreateCarAsync(fiat.Id, red.Id, "Egea");
        await CreateCarAsync(toyota.Id, red.Id, "Second Corolla");

        var first = await PageAsync<CarResponse>($"/api/v2/cars?brandId={toyota.Id}&limit=1");
        var second = await PageAsync<CarResponse>($"/api/v2/cars?brandId={toyota.Id}&limit=1&cursor={first.Page.NextCursor}");

        Assert.Equal("First Corolla", Assert.Single(first.Items).Description);
        Assert.True(first.Page.HasMore);
        Assert.Equal("Second Corolla", Assert.Single(second.Items).Description);
        Assert.False(second.Page.HasMore);
    }

    [Fact]
    public async Task UsersByEmail_AnswerAPageLikeAnyOtherList()
    {
        await CreateUserAsync("first@example.com");
        var wanted = await CreateUserAsync("second@example.com");

        var page = await PageAsync<UserResponse>("/api/v2/users?email=second@example.com");

        Assert.Equal(wanted.Id, Assert.Single(page.Items).Id);
        Assert.False(page.Page.HasMore);
    }

    [Fact]
    public async Task CompanyLists_AnswerOnePage_OfRolesAndOfEmployeesWithTheirRoles()
    {
        var member = await CreateUserAsync("member@example.com", "Member");
        await SignInAsNewUserAsync("owner@example.com", "Owner");
        var company = await RegisterCompanyAsync();
        await AddEmployeeAsync(company.Id, member.Id, "Member");

        var roles = await PageAsync<RoleBody>($"/api/v2/companies/{company.Id}/roles?limit=2");
        Assert.Equal(["Owner", "Admin"], roles.Items.Select(r => r.Name));
        Assert.True(roles.Page.HasMore);

        var owners = await PageAsync<EmployeeBody>($"/api/v2/companies/{company.Id}/employees?limit=1");
        var members = await PageAsync<EmployeeBody>($"/api/v2/companies/{company.Id}/employees?limit=1&cursor={owners.Page.NextCursor}");
        var employee = Assert.Single(members.Items);
        Assert.Equal(member.Id, employee.UserId);
        Assert.Equal("Member", employee.FirstName);
        Assert.Equal("Member", Assert.Single(employee.Roles).Name);
        Assert.False(members.Page.HasMore);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task LimitOutOfRange_ReturnsValidationProblem(int limit)
    {
        var response = await Client.GetJsonAsync($"/api/v2/brands?limit={limit}");

        await AssertValidationProblemAsync(response, "limit");
    }

    [Theory]
    [InlineData("not-a-cursor!")]
    [InlineData("aGVsbG8")] // hello
    [InlineData("e30")] // {}
    [InlineData("eyJpZCI6IngifQ")] // {"id":"x"}
    public async Task CursorThisApiDidNotWrite_ReturnsValidationProblem(string cursor)
    {
        var response = await Client.GetJsonAsync($"/api/v2/brands?cursor={cursor}");

        await AssertValidationProblemAsync(response, "cursor");
    }

    [Fact]
    public async Task LargestLimit_IsAllowed()
    {
        await CreateBrandsAsync(30);

        var page = await PageAsync<BrandBody>("/api/v2/brands?limit=100");

        Assert.Equal(30, page.Items.Count);
        Assert.False(page.Page.HasMore);
    }

    [Fact]
    public async Task V1_StillListsEverything_InItsEnvelope()
    {
        await CreateBrandsAsync(30);

        var response = await Client.GetJsonAsync("/api/v1/brands/getall");

        var body = await response.ReadAsAsync<V1Response<List<BrandBody>>>();
        Assert.Equal(30, body.Data.Count);
    }

    private async Task<PagedResponse<T>> PageAsync<T>(string url)
    {
        var response = await Client.GetJsonAsync(url);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.ReadAsAsync<PagedResponse<T>>();
    }

    private async Task CreateBrandsAsync(int count)
    {
        for (var number = 1; number <= count; number++)
        {
            await CreateBrandAsync($"Brand {number:D2}");
        }
    }
}
