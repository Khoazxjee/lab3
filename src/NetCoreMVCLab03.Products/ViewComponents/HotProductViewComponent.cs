using Microsoft.AspNetCore.Mvc;
using NetCoreMVCLab03.Products.Services;

namespace NetCoreMVCLab03.Products.ViewComponents;

public class HotProductViewComponent(ProductCatalog catalog) : ViewComponent
{
    public IViewComponentResult Invoke() => View(catalog.GetHotProducts());
}
