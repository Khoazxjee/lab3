using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Products.Models;
using NetCoreMVCLab03.Products.Services;

namespace NetCoreMVCLab03.Products.Controllers;

public class HomeController(ProductCatalog catalog) : Controller
{
    public IActionResult Index() => View(catalog.GetLatestProducts());
    public IActionResult About() => View();
    public IActionResult Contact() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
