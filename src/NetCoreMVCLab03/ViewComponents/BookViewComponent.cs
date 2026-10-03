using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Services;

namespace NetCoreMVCLab03.ViewComponents;

public class BookViewComponent : ViewComponent
{
    private readonly BookCatalog _catalog;
    public BookViewComponent(BookCatalog catalog) => _catalog = catalog;
    public IViewComponentResult Invoke() => View(_catalog.GetBookList());
}
