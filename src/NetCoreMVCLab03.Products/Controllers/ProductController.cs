using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Products.Services;

namespace NetCoreMVCLab03.Products.Controllers;

public class ProductController(ProductCatalog catalog) : Controller
{
    public IActionResult Index(int? categoryId)
    {
        if (!ModelState.IsValid) return BadRequest();
        if (categoryId.HasValue && !catalog.Categories.Any(c => c.Id == categoryId)) return NotFound();
        ViewBag.CategoryId = categoryId;
        ViewBag.CategoryName = catalog.Categories.FirstOrDefault(c => c.Id == categoryId)?.Name;
        return View(catalog.Products.Where(p => !categoryId.HasValue || p.CategoryId == categoryId));
    }

    public IActionResult Details(int id)
    {
        var product = catalog.Products.FirstOrDefault(p => p.Id == id);
        return product is null ? NotFound() : View(product);
    }
}
