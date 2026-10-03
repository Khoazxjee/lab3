using System.Net;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NetCoreMVCLab03.Tests;

public class ProductAppTests : IClassFixture<WebApplicationFactory<Products.Program>>
{
    private readonly HttpClient _client;
    public ProductAppTests(WebApplicationFactory<Products.Program> factory) => _client = factory.CreateClient();

    [Fact]
    public async Task HomeSeparatesActionAndComponentProductsAndLoadsCategories()
    {
        using var page = new HtmlParser().ParseDocument(await _client.GetStringAsync("/"));
        Assert.Equal(new[] { "1", "2", "3" }, page.QuerySelectorAll("#latest-products [data-product-id]").Select(e => e.GetAttribute("data-product-id")));
        Assert.Equal(new[] { "4", "5", "6" }, page.QuerySelectorAll("#hot-products [data-product-id]").Select(e => e.GetAttribute("data-product-id")));
        Assert.Equal(11, page.QuerySelectorAll(".category-list nav a").Length);
        Assert.Contains("Áo dài", page.QuerySelector("aside")!.TextContent);
        Assert.Contains("GIỚI THIỆU", page.QuerySelector("footer")!.TextContent);
    }

    [Theory]
    [InlineData("/Home/About", 200)]
    [InlineData("/Home/Contact", 200)]
    [InlineData("/Product", 200)]
    [InlineData("/Product/Details/1", 200)]
    [InlineData("/Product/Details/999", 404)]
    [InlineData("/Product?categoryId=999", 404)]
    [InlineData("/Product?categoryId=invalid", 400)]
    public async Task NavigationReturnsExpectedStatus(string path, int status) => Assert.Equal((HttpStatusCode)status, (await _client.GetAsync(path)).StatusCode);

    [Theory]
    [InlineData("/Product", 6)]
    [InlineData("/Product?categoryId=11", 6)]
    [InlineData("/Product?categoryId=1", 0)]
    public async Task CategoryFilteringWorks(string path, int count)
    {
        using var page = new HtmlParser().ParseDocument(await _client.GetStringAsync(path));
        Assert.Equal(count, page.QuerySelectorAll("[data-product-id]").Length);
        if (count == 0) Assert.NotNull(page.QuerySelector(".empty-state"));
    }
}
