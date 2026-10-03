# Đối chiếu Lab 03

## Phần hướng dẫn

| Nội dung đề | File triển khai chính |
| --- | --- |
| Model Book: Id, Title, AuthorId, GenreId, Image, Price, TotalPage, Sumary | `src/NetCoreMVCLab03/Models/Book.cs` |
| Danh sách sách và dữ liệu mẫu | `Models/Book.cs`, `Services/BookCatalog.cs`, `Controllers/BookController.cs`, `Views/Book/Index.cshtml` |
| Form Create và Edit; dropdown tác giả/thể loại; tag helpers | `Views/Book/Create.cshtml`, `Edit.cshtml`, `_BookForm.cshtml` |
| Partial View banner và gọi trong Home | `Views/Shared/Banner.cshtml`, `Views/Home/Index.cshtml` |
| PopularBook Partial View qua AJAX | `BookController.PopularBook()`, `Views/Book/PopularBook.cshtml`, `wwwroot/js/home.js` |
| Book View Component | `ViewComponents/BookViewComponent.cs`, `Views/Shared/Components/Book/Default.cshtml` |
| Layout tự tạo và layout mặc định | `Views/Shared/my_layout.cshtml`, `Views/_ViewStart.cshtml` |

Các đường dẫn ngắn ở bảng trên tính từ `src/NetCoreMVCLab03`.

## Bài tập tự làm

Project riêng: `src/NetCoreMVCLab03.Products`.

| Nội dung đề | File triển khai |
| --- | --- |
| Layout navbar, sidebar, nội dung, footer | `Views/Shared/my_layout.cshtml`, `Views/_ViewStart.cshtml` |
| Danh mục sản phẩm từ Partial View | `Views/Shared/Category.cshtml` |
| Sản phẩm mới từ View Action | `Controllers/HomeController.cs`, `Views/Home/Index.cshtml` |
| Sản phẩm nổi bật từ Component | `ViewComponents/HotProductViewComponent.cs`, `Views/Shared/Components/HotProduct/Default.cshtml` |
| Ảnh và tên sản phẩm mẫu | `Models/Product.cs`, `Services/ProductCatalog.cs`, `wwwroot/images/nag0102.jpg` |

Không dựng lại các mũi tên/chú thích màu vàng trong ảnh đề: đó là chú thích kỹ thuật của tài liệu, không phải thành phần giao diện.

## Kịch bản kiểm tra

1. Chạy project sách, vào `/Book`. Kiểm tra 4 dòng Id 1, 2, 4, 6 và các ảnh bìa.
2. Bấm **Thêm mới**, lưu form trống: thấy thông báo lỗi. Điền thông tin hợp lệ, tác giả Ngô Tất Tố, thể loại Văn học đương đại, chọn ảnh JPG rồi lưu: về danh sách, xuất hiện Id 7.
3. Bấm **Sửa** Id 7, đổi tên và lưu mà không chọn lại ảnh: tên đổi, ảnh được giữ nguyên.
4. Lọc theo tác giả 2 và thể loại 2: có sách vừa thêm. Đổi thể loại thành 1: kết quả trống. Xóa cả hai bộ lọc: hiện tất cả.
5. Tắt và chạy lại server: bản ghi mới vẫn còn.
6. Trang chủ: trước khi bấm nút, chỉ có danh sách Component. Bấm **Tải sách phổ biến**: xuất hiện danh sách AJAX. Bấm lần nữa không nhân đôi danh sách.
7. Chuyển banner bằng mũi tên, thử nút dừng/tiếp tục.
8. Chạy project sản phẩm: sidebar bên trái, 3 sản phẩm mới và 3 sản phẩm nổi bật bên phải, footer hai cột.
9. Bấm danh mục **Nồi cơm điện**: hiện 6 bản ghi mẫu; bấm **Áo dài**: trạng thái chưa có sản phẩm. Bấm sản phẩm để xem chi tiết.
10. Kiểm tra ở màn hình nhỏ: menu thu gọn, form vừa chiều ngang, bảng sách cuộn ngang bên trong vùng bảng, các card sản phẩm xuống hàng.

## Kiểm thử tự động

```powershell
dotnet test NetCoreMVCLab03.sln --configuration Release
```

Kiểm thử chạy trên TestServer với thư mục dữ liệu/ảnh tạm riêng cho từng test, không ghi đè dữ liệu người dùng. Bao phủ: route, layout, dữ liệu mẫu, AJAX partial, Component, thêm/sửa và lưu JSON, giữ/thay ảnh, lọc kết hợp, input không hợp lệ, file sai/ quá lớn, CSRF, HTML escaping và liên kết form sửa.

Ảnh trong `screenshots` chụp từ ứng dụng thật. Một số ảnh form/mobile có bản ghi mang tên **Sách kiểm thử đã sửa** để minh họa việc lưu; bản ghi kiểm thử không có trong dữ liệu mặc định của bài nộp.
