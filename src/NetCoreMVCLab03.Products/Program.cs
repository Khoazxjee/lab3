using NetCoreMVCLab03.Products.Services;

namespace NetCoreMVCLab03.Products;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllersWithViews();
        builder.Services.AddSingleton<ProductCatalog>();
        var app = builder.Build();
        if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        app.Run();
    }
}
