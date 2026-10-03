using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Models;
using NetCoreMVCLab03.Services;

namespace NetCoreMVCLab03.Controllers;

public class BookController : Controller
{
    private readonly BookCatalog _catalog;
    private readonly BookImages _images;
    private readonly Book _book = new();
    public BookController(BookCatalog catalog, BookImages images) { _catalog = catalog; _images = images; }

    [HttpGet]
    public IActionResult Index(string? authorId, int? genreId)
    {
        if (!ModelState.IsValid || !ValidFilters(authorId, genreId)) return BadRequest("Tác giả hoặc thể loại không hợp lệ.");
        Choices();
        ViewBag.AuthorId = authorId;
        ViewBag.GenreId = genreId;
        IEnumerable<Book> books = _catalog.GetBookList();
        if (!string.IsNullOrEmpty(authorId)) books = books.Where(book => book.AuthorId == authorId);
        if (genreId.HasValue) books = books.Where(book => book.GenreId == genreId);
        return View(books.ToList());
    }

    [HttpPost, ActionName("Index"), ValidateAntiForgeryToken]
    public IActionResult Search(string? authorId, int? genreId)
    {
        if (!ModelState.IsValid || !ValidFilters(authorId, genreId)) return BadRequest("Tác giả hoặc thể loại không hợp lệ.");
        return RedirectToAction(nameof(Index), new { authorId, genreId });
    }

    public IActionResult Create() { Choices(); return View(new Book()); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,AuthorId,GenreId,Price,TotalPage,Sumary")] Book model, [FromForm(Name = "Image")] IFormFile? imageFile)
    {
        await ValidateBook(model, imageFile, null);
        if (!ModelState.IsValid) { Choices(); return View(model); }
        model.Image = await _images.SaveAsync(imageFile!);
        try { _catalog.Add(model); }
        catch { _images.Delete(model.Image); throw; }
        TempData["Success"] = "Đã thêm sách thành công.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Edit(int id)
    {
        var model = _catalog.GetBookById(id);
        if (model is null) return NotFound();
        Choices();
        return View(model);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([FromRoute] int id, [Bind("Id,Title,AuthorId,GenreId,Price,TotalPage,Sumary")] Book model, [FromForm(Name = "Image")] IFormFile? imageFile)
    {
        if (id != model.Id) return BadRequest();
        var existing = _catalog.GetBookById(id);
        if (existing is null) return NotFound();
        await ValidateBook(model, imageFile, existing.Image);
        if (!ModelState.IsValid) { Choices(); return View(model); }
        string image = imageFile is null ? existing.Image : await _images.SaveAsync(imageFile);
        model.Image = image;
        try
        {
            if (!_catalog.Update(model)) { if (image != existing.Image) _images.Delete(image); return NotFound(); }
        }
        catch { if (image != existing.Image) _images.Delete(image); throw; }
        if (image != existing.Image) _images.Delete(existing.Image);
        TempData["Success"] = "Đã cập nhật sách.";
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Details(int id)
    {
        var book = _catalog.GetBookById(id);
        if (book is null) return NotFound();
        return View(book);
    }

    public PartialViewResult PopularBook() => PartialView(_catalog.GetBookList());

    private void Choices() { ViewBag.authors = _book.Authors; ViewBag.genres = _book.Genres; }
    private bool ValidFilters(string? authorId, int? genreId) =>
        (string.IsNullOrEmpty(authorId) || _book.Authors.Any(item => item.Value == authorId)) &&
        (!genreId.HasValue || _book.Genres.Any(item => item.Value == genreId.Value.ToString()));

    private async Task ValidateBook(Book model, IFormFile? imageFile, string? existing)
    {
        if (!float.IsFinite(model.Price)) ModelState.AddModelError(nameof(Book.Price), "Giá sách phải là một số hữu hạn");
        model.Image = existing ?? "";
        if (imageFile is null)
        {
            if (existing is null) ModelState.AddModelError(nameof(Book.Image), "Vui lòng chọn ảnh sách");
            return;
        }
        string? error = await _images.ValidateAsync(imageFile);
        if (error is not null) ModelState.AddModelError(nameof(Book.Image), error);
    }
}
