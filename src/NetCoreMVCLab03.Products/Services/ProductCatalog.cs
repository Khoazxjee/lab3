using NetCoreMVCLab03.Products.Models;

namespace NetCoreMVCLab03.Products.Services;

public class ProductCatalog
{
    public IReadOnlyList<Category> Categories { get; } = new List<Category>
    {
        new(1, "Áo dài"), new(2, "Áo đông"), new(3, "Túi xách"),
        new(4, "Đồng hồ"), new(5, "Ví da"), new(6, "Thắt lưng da"),
        new(7, "Tủ lạnh"), new(8, "Tivi"), new(9, "Quạt điện"),
        new(10, "Lò sưởi"), new(11, "Nồi cơm điện")
    }.AsReadOnly();

    public IReadOnlyList<Product> Products { get; } = Enumerable.Range(1, 6)
        .Select(id => new Product(id, "Nồi cơm điện cao tần Nagakawa NAG0102",
            "/images/nag0102.jpg", 11, id > 3)).ToList().AsReadOnly();

    public IEnumerable<Product> GetLatestProducts() => Products.Where(p => !p.IsHot);
    public IEnumerable<Product> GetHotProducts() => Products.Where(p => p.IsHot);
}
