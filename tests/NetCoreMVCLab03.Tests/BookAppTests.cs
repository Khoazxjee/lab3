using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Parser;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetCoreMVCLab03.Services;

namespace NetCoreMVCLab03.Tests;

public class BookAppTests : IDisposable
{
    private readonly string _temp = Path.Combine(Path.GetTempPath(), "lab03-tests-" + Guid.NewGuid());
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private BookCatalog Catalog => _factory.Services.GetRequiredService<BookCatalog>();
    private string Uploads => Path.Combine(_temp, "wwwroot", "images", "products", "uploads");

    public BookAppTests()
    {
        Directory.CreateDirectory(Path.Combine(_temp, "wwwroot"));
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseWebRoot(Path.Combine(_temp, "wwwroot"));
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(
                new Dictionary<string, string?> { ["DataDirectory"] = _temp }));
        });
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    private async Task<IDocument> Page(string url) => new HtmlParser().ParseDocument(await _client.GetStringAsync(url));
    private static Dictionary<string, string> Fields(int id = 0) => new()
    {
        ["Id"] = id.ToString(), ["Title"] = "Sách kiểm thử", ["AuthorId"] = "2", ["GenreId"] = "2",
        ["Price"] = "125000.5", ["TotalPage"] = "250", ["Sumary"] = "Nội dung tiếng Việt"
    };

    private async Task<HttpResponseMessage> Submit(string path, Dictionary<string, string>? fields = null,
        bool image = false, string fileName = "cover.png", byte[]? bytes = null)
    {
        string getPath = path.StartsWith("/Book/Edit/") ? path : "/Book/Create";
        using var page = await Page(getPath);
        string token = page.QuerySelector("input[name='__RequestVerificationToken']")!.GetAttribute("value")!;
        using var body = new MultipartFormDataContent();
        foreach (var pair in fields ?? Fields()) body.Add(new StringContent(pair.Value), pair.Key);
        body.Add(new StringContent(token), "__RequestVerificationToken");
        if (image)
        {
            bytes ??= Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+jf5sAAAAASUVORK5CYII=");
            body.Add(new ByteArrayContent(bytes), "Image", fileName);
        }
        return await _client.PostAsync(path, body);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/Home/About")]
    [InlineData("/Home/Contact")]
    [InlineData("/Book")]
    [InlineData("/Book/Create")]
    [InlineData("/Book/Edit/2")]
    [InlineData("/Book/Details/6")]
    public async Task PagesRender(string path)
    {
        using var response = await _client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("text/html", response.Content.Headers.ContentType!.ToString());
    }

    [Fact]
    public async Task HomeHasBannerComponentAndAjaxTarget()
    {
        using var page = await Page("/");
        Assert.Equal(3, page.QuerySelectorAll(".carousel-item").Length);
        Assert.Equal(4, page.QuerySelectorAll("#books-from-component [data-book-id]").Length);
        Assert.Empty(page.QuerySelectorAll("#data-from-ajax [data-book-id]"));
        Assert.Equal("/Book/PopularBook", page.QuerySelector("#loadAjax")!.GetAttribute("data-url"));
    }

    [Fact]
    public async Task PopularBooksReturnsOnlyPartialAndCurrentData()
    {
        using var response = await Submit("/Book/Create", image: true);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string html = await _client.GetStringAsync("/Book/PopularBook");
        using var page = new HtmlParser().ParseDocument(html);
        Assert.DoesNotContain("<html", html);
        Assert.DoesNotContain("navbar", html);
        Assert.Equal(5, page.QuerySelectorAll("[data-book-id]").Length);
    }

    [Fact]
    public async Task IndexAndEditMatchSeedData()
    {
        using var index = await Page("/Book");
        Assert.Equal(new[] { "1", "2", "4", "6" }, index.QuerySelectorAll("tbody [data-book-id]").Select(e => e.GetAttribute("data-book-id")));
        using var edit = await Page("/Book/Edit/2");
        Assert.Equal("Lão Hạc", edit.QuerySelector("#Title")!.GetAttribute("value"));
        Assert.Equal("1", edit.QuerySelector("#AuthorId option[selected]")!.GetAttribute("value"));
        Assert.Equal("700000", edit.QuerySelector("#Price")!.GetAttribute("value"));
        Assert.Equal("multipart/form-data", edit.QuerySelector("form")!.GetAttribute("enctype"));
        Assert.Equal("/Book/Edit/2", edit.QuerySelector("form")!.GetAttribute("action"));
        Assert.Equal("2", edit.QuerySelector("#Id")!.GetAttribute("value"));
        Assert.False(edit.QuerySelector("#Image")!.HasAttribute("data-val-required"));
        Assert.Equal("/images/products/b2.jpg", edit.QuerySelector("#imagePreview")!.GetAttribute("src"));
    }

    [Fact]
    public async Task DetailsLinksToTheCorrectEditForm()
    {
        using var page = await Page("/Book/Details/2");
        Assert.Equal("/Book/Edit/2", page.QuerySelector(".form-actions .btn")!.GetAttribute("href"));
    }

    [Fact]
    public async Task CreateSavesUnicodeAndUploadedImageThenSurvivesReload()
    {
        using var response = await Submit("/Book/Create", image: true, fileName: "../../ảnh.png");
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        var book = Catalog.GetBookById(7)!;
        Assert.Equal("Sách kiểm thử", book.Title);
        Assert.Equal(125000.5f, book.Price);
        Assert.StartsWith("/images/products/uploads/", book.Image);
        Assert.DoesNotContain("..", book.Image);
        Assert.Single(Directory.GetFiles(Uploads));
        var reloaded = new BookCatalog(_factory.Services.GetRequiredService<IWebHostEnvironment>(),
            _factory.Services.GetRequiredService<IConfiguration>());
        Assert.Equal(book.Image, reloaded.GetBookById(7)!.Image);
        Assert.Equal(book.Sumary, reloaded.GetBookById(7)!.Sumary);
    }

    [Fact]
    public async Task EditRetainsImageAndOnlyUpdatesChosenBook()
    {
        var fields = Fields(2);
        fields["Image"] = "/arbitrary.png";
        using var response = await Submit("/Book/Edit/2", fields);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/images/products/b2.jpg", Catalog.GetBookById(2)!.Image);
        Assert.Equal("Sách kiểm thử", Catalog.GetBookById(2)!.Title);
        Assert.Equal("Chí Phèo", Catalog.GetBookById(1)!.Title);
        Assert.Equal(4, Catalog.GetBookList().Count);
    }

    [Fact]
    public async Task ReplaceUploadedImageRemovesOnlyPreviousUpload()
    {
        using var create = await Submit("/Book/Create", image: true);
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);
        string oldImage = Catalog.GetBookById(7)!.Image;
        using var edit = await Submit("/Book/Edit/7", Fields(7), image: true);
        Assert.Equal(HttpStatusCode.Redirect, edit.StatusCode);
        Assert.NotEqual(oldImage, Catalog.GetBookById(7)!.Image);
        Assert.Single(Directory.GetFiles(Uploads));
        Assert.False(File.Exists(Path.Combine(Uploads, Path.GetFileName(oldImage))));
    }

    [Theory]
    [InlineData("Title", "")]
    [InlineData("AuthorId", "99")]
    [InlineData("GenreId", "5")]
    [InlineData("Price", "-1")]
    [InlineData("Price", "abc")]
    [InlineData("Price", "NaN")]
    [InlineData("Price", "Infinity")]
    [InlineData("TotalPage", "0")]
    [InlineData("TotalPage", "1.5")]
    [InlineData("TotalPage", "")]
    public async Task InvalidInputDoesNotSave(string key, string value)
    {
        var fields = Fields();
        fields[key] = value;
        using var response = await Submit("/Book/Create", fields, image: true);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var page = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(page.QuerySelectorAll(".field-validation-error"));
        Assert.Equal(4, Catalog.GetBookList().Count);
        Assert.Empty(Directory.GetFiles(Uploads));
    }

    [Fact]
    public async Task MissingPriceAndImageAreRejected()
    {
        var fields = Fields();
        fields.Remove("Price");
        using var response = await Submit("/Book/Create", fields);
        using var page = new HtmlParser().ParseDocument(await response.Content.ReadAsStringAsync());
        Assert.NotEmpty(page.QuerySelector("[data-valmsg-for='Price']")!.TextContent);
        Assert.NotEmpty(page.QuerySelector("[data-valmsg-for='Image']")!.TextContent);
        Assert.Equal(4, Catalog.GetBookList().Count);
    }

    [Theory]
    [InlineData("malware.svg", false)]
    [InlineData("fake.jpg", false)]
    [InlineData("large.png", true)]
    public async Task UnsupportedOrOversizeImageIsRejected(string name, bool oversize)
    {
        byte[] bytes = oversize ? new byte[5 * 1024 * 1024 + 1] : "<script>alert(1)</script>"u8.ToArray();
        using var response = await Submit("/Book/Create", image: true, fileName: name, bytes: bytes);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(4, Catalog.GetBookList().Count);
        Assert.Empty(Directory.GetFiles(Uploads));
    }

    [Fact]
    public async Task SearchCombinesAuthorAndGenreAndPreservesSelection()
    {
        using var create = await Submit("/Book/Create", image: true);
        Assert.Equal(HttpStatusCode.Redirect, create.StatusCode);
        using var found = await Page("/Book?authorId=2&genreId=2");
        Assert.Single(found.QuerySelectorAll("tbody [data-book-id]"));
        Assert.Equal("2", found.QuerySelector("select[name='AuthorId'] option[selected]")!.GetAttribute("value"));
        using var empty = await Page("/Book?authorId=2&genreId=1");
        Assert.Empty(empty.QuerySelectorAll("tbody [data-book-id]"));
    }

    [Fact]
    public async Task SearchPostRedirectsToFilteredGet()
    {
        using var response = await Submit("/Book/Index", new() { ["AuthorId"] = "1", ["GenreId"] = "1" });
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Equal("/Book?authorId=1&genreId=1", response.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("/Book?authorId=99")]
    [InlineData("/Book?genreId=wrong")]
    [InlineData("/Book?genreId=8")]
    public async Task InvalidFiltersReturnBadRequest(string url) => Assert.Equal(HttpStatusCode.BadRequest, (await _client.GetAsync(url)).StatusCode);

    [Theory]
    [InlineData("/Book/Edit/999")]
    [InlineData("/Book/Details/999")]
    public async Task MissingBookReturns404(string url) => Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync(url)).StatusCode);

    [Fact]
    public async Task EditRejectsMismatchedRouteId()
    {
        using var response = await Submit("/Book/Edit/2", Fields(1));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Lão Hạc", Catalog.GetBookById(2)!.Title);
    }

    [Fact]
    public async Task WritesRequireAntiforgeryToken()
    {
        using var response = await _client.PostAsync("/Book/Create", new FormUrlEncodedContent(Fields()));
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(4, Catalog.GetBookList().Count);
    }

    [Fact]
    public async Task HtmlInBookTitleIsEscaped()
    {
        var fields = Fields();
        fields["Title"] = "<script>alert('test')</script>";
        using var response = await Submit("/Book/Create", fields, image: true);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        string html = await _client.GetStringAsync("/Book");
        Assert.DoesNotContain(fields["Title"], html);
        using var page = new HtmlParser().ParseDocument(html);
        Assert.Contains(fields["Title"], page.QuerySelector("tbody")!.TextContent);
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
        if (Directory.Exists(_temp)) Directory.Delete(_temp, recursive: true);
    }
}
