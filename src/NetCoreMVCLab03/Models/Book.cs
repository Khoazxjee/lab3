using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace NetCoreMVCLab03.Models;

public class Book
{
    public int Id { get; set; }

    [Display(Name = "Tên sách")]
    [Required(ErrorMessage = "Tên sách không được để trống")]
    [StringLength(200, ErrorMessage = "Tên sách không vượt quá 200 ký tự")]
    public string Title { get; set; } = "";

    [Display(Name = "Tác giả")]
    [Required(ErrorMessage = "Vui lòng chọn tác giả")]
    [RegularExpression("^[1-4]$", ErrorMessage = "Tác giả không có trong danh sách")]
    public string AuthorId { get; set; } = "";

    [Display(Name = "Thể loại")]
    [Range(1, 4, ErrorMessage = "Vui lòng chọn thể loại trong danh sách")]
    public int GenreId { get; set; }

    [Display(Name = "Hình ảnh/ Poster")]
    [BindNever, ValidateNever]
    public string Image { get; set; } = "";

    [Display(Name = "Giá sách")]
    [BindRequired]
    [Range(0, double.MaxValue, ErrorMessage = "Giá sách phải là số không âm")]
    public float Price { get; set; }

    [Display(Name = "Tổng số trang")]
    [BindRequired]
    [Range(1, int.MaxValue, ErrorMessage = "Tổng số trang phải là số nguyên lớn hơn 0")]
    public int TotalPage { get; set; }

    [Display(Name = "Giới thiệu ngắn")]
    [StringLength(4000, ErrorMessage = "Giới thiệu không vượt quá 4000 ký tự")]
    public string? Sumary { get; set; }

    [JsonIgnore, ValidateNever]
    public List<SelectListItem> Authors => new()
    {
        new("Nam Cao", "1"), new("Ngô Tất Tố", "2"),
        new("Adam Khoo", "3"), new("Thiền sư Thích Nhất Hạnh", "4")
    };

    [JsonIgnore, ValidateNever]
    public List<SelectListItem> Genres => new()
    {
        new("Truyện tranh", "1"), new("Văn học đương đại", "2"),
        new("Phật học phổ thông", "3"), new("Truyện cười", "4")
    };

    public List<Book> GetBookList() => new()
    {
        new() { Id = 1, Title = "Chí Phèo", AuthorId = "1", GenreId = 1, Image = "/images/products/b1.jpg", Price = 500000, TotalPage = 250, Sumary = "" },
        new() { Id = 2, Title = "Lão Hạc", AuthorId = "1", GenreId = 1, Image = "/images/products/b2.jpg", Price = 700000, TotalPage = 400, Sumary = "" },
        new() { Id = 4, Title = "Conan Phiêu lưu ký", AuthorId = "1", GenreId = 1, Image = "/images/products/b3.jpg", Price = 550000, TotalPage = 320, Sumary = "" },
        new() { Id = 6, Title = "Đường Xưa Mây Trắng", AuthorId = "1", GenreId = 1, Image = "/images/products/b4.jpg", Price = 850000, TotalPage = 600, Sumary = "" }
    };

    public Book? GetBookById(int id) => GetBookList().FirstOrDefault(book => book.Id == id);

    public Book Copy() => new() { Id = Id, Title = Title, AuthorId = AuthorId, GenreId = GenreId, Image = Image, Price = Price, TotalPage = TotalPage, Sumary = Sumary };
}
