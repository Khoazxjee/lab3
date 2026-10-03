using Microsoft.AspNetCore.Http.Features;
using NetCoreMVCLab03.Services;

namespace NetCoreMVCLab03;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddSingleton<BookCatalog>();
        builder.Services.AddSingleton<BookImages>();
        builder.Services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = 10 * 1024 * 1024);
        builder.Services.AddControllersWithViews(options =>
        {
            var messages = options.ModelBindingMessageProvider;
            messages.SetValueMustNotBeNullAccessor(_ => "Vui lòng nhập thông tin bắt buộc.");
            messages.SetAttemptedValueIsInvalidAccessor((value, name) => $"Giá trị '{value}' không hợp lệ cho {name}.");
            messages.SetValueMustBeANumberAccessor(name => $"{name} phải là một số.");
            messages.SetMissingBindRequiredValueAccessor(name => $"Vui lòng nhập {name}.");
        });
        var app = builder.Build();
        if (!app.Environment.IsDevelopment()) app.UseExceptionHandler("/Home/Error");
        app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => context.Context.Response.Headers["X-Content-Type-Options"] = "nosniff" });
        app.UseRequestLocalization("en-US");
        app.UseRouting();
        app.UseAuthorization();
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
        app.Run();
    }
}
