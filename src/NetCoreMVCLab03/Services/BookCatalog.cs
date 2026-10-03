using System.Text.Json;
using NetCoreMVCLab03.Models;

namespace NetCoreMVCLab03.Services;

public sealed class BookCatalog
{
    private readonly object _sync = new();
    private readonly string _path;
    private List<Book> _books;

    public BookCatalog(IWebHostEnvironment environment, IConfiguration configuration)
    {
        string directory = configuration["DataDirectory"] ?? Path.Combine(environment.ContentRootPath, "App_Data");
        Directory.CreateDirectory(directory);
        _path = Path.Combine(directory, "books.json");
        _books = File.Exists(_path)
            ? JsonSerializer.Deserialize<List<Book>>(File.ReadAllText(_path)) ?? throw new InvalidDataException("Dữ liệu sách không hợp lệ.")
            : new Book().GetBookList();
    }

    public List<Book> GetBookList() { lock (_sync) return _books.Select(book => book.Copy()).ToList(); }
    public Book? GetBookById(int id) { lock (_sync) return _books.FirstOrDefault(book => book.Id == id)?.Copy(); }

    public void Add(Book book)
    {
        lock (_sync)
        {
            var next = _books.Select(item => item.Copy()).ToList();
            book.Id = next.Select(item => item.Id).DefaultIfEmpty().Max() + 1;
            next.Add(book.Copy());
            Save(next);
        }
    }

    public bool Update(Book book)
    {
        lock (_sync)
        {
            var next = _books.Select(item => item.Copy()).ToList();
            int index = next.FindIndex(item => item.Id == book.Id);
            if (index < 0) return false;
            next[index] = book.Copy();
            Save(next);
            return true;
        }
    }

    private void Save(List<Book> next)
    {
        File.WriteAllText(_path + ".tmp", JsonSerializer.Serialize(next, new JsonSerializerOptions { WriteIndented = true }));
        File.Move(_path + ".tmp", _path, overwrite: true);
        _books = next;
    }
}
