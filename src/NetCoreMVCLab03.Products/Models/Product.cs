namespace NetCoreMVCLab03.Products.Models;

public record Product(int Id, string Name, string Image, int CategoryId, bool IsHot);

public record Category(int Id, string Name);
