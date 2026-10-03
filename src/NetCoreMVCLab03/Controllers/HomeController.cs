using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Models;

namespace NetCoreMVCLab03.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
    public IActionResult About() => View();
    public IActionResult Contact() => View();
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
