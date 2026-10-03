namespace NetCoreMVCLab03.Services;

public sealed class BookImages
{
    private const string Prefix = "/images/products/uploads/";
    private readonly string _directory;
    private readonly ILogger<BookImages> _logger;
    public BookImages(IWebHostEnvironment environment, ILogger<BookImages> logger)
    {
        _directory = Path.Combine(environment.WebRootPath, "images", "products", "uploads");
        Directory.CreateDirectory(_directory);
        _logger = logger;
    }

    public async Task<string?> ValidateAsync(IFormFile file)
    {
        if (file.Length == 0 || file.Length > 5 * 1024 * 1024) return "Ảnh phải có dung lượng từ 1 byte đến 5 MB";
        string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        int size = 0;
        while (size < header.Length)
        {
            int count = await stream.ReadAsync(header.AsMemory(size));
            if (count == 0) break;
            size += count;
        }
        bool valid = extension switch
        {
            ".png" => size >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }),
            ".jpg" or ".jpeg" => size >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255,
            ".webp" => size >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
        return valid ? null : "Vui lòng chọn ảnh JPG, PNG hoặc WebP hợp lệ";
    }

    public async Task<string> SaveAsync(IFormFile file)
    {
        string name = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName).ToLowerInvariant();
        try
        {
            await using var stream = new FileStream(Path.Combine(_directory, name), FileMode.CreateNew);
            await file.CopyToAsync(stream);
            return Prefix + name;
        }
        catch { Delete(Prefix + name); throw; }
    }

    public void Delete(string image)
    {
        if (!image.StartsWith(Prefix, StringComparison.Ordinal)) return;
        string name = image[Prefix.Length..];
        if (string.IsNullOrWhiteSpace(name) || name != Path.GetFileName(name)) return;
        try { File.Delete(Path.Combine(_directory, name)); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _logger.LogWarning(ex, "Không dọn được ảnh sách cũ."); }
    }
}
