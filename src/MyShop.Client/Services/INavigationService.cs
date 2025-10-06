using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Interface định nghĩa các hoạt động navigation service để quản lý trang trong ứng dụng.
    /// Cung cấp các hợp đồng để khởi tạo điều hướng và điều hướng giữa các trang khác nhau.
    /// </summary>
    /// <remarks>
    /// Interface này trừu tượng hóa các hoạt động điều hướng trang, cho phép:
    /// - Dependency injection và kiểm thử thông qua các implementation giả lập
    /// - API điều hướng nhất quán trên các implementation khác nhau
    /// - Tách biệt rõ ràng giữa logic điều hướng và các thành phần UI
    /// - Kiểm thử unit dễ dàng cho các ViewModel phụ thuộc vào điều hướng
    /// - Khả năng mở rộng trong tương lai cho các mẫu điều hướng khác nhau
    /// 
    /// Navigation service hoạt động với WinUI Frame controls và Page types,
    /// cung cấp cả khả năng điều hướng type-safe và runtime.
    /// </remarks>
    public interface INavigationService
    {
        /// <summary>
        /// Khởi tạo navigation service với Frame control được chỉ định.
        /// Phương thức này phải được gọi trước khi có thể thực hiện bất kỳ hoạt động điều hướng nào.
        /// </summary>
        /// <param name="frame">
        /// WinUI Frame control sẽ được sử dụng cho điều hướng trang.
        /// Thường là frame ứng dụng chính hoặc một frame điều hướng cụ thể.
        /// Không được null.
        /// </param>
        /// <remarks>
        /// Implementation nên:
        /// - Lưu trữ tham chiếu frame cho các hoạt động điều hướng sau này
        /// - Validate rằng tham số frame không phải null
        /// - Chuẩn bị service cho các lời gọi điều hướng tiếp theo
        /// - Có thể thiết lập bất kỳ event handler điều hướng nào nếu cần
        /// 
        /// Phương thức này thường được gọi một lần trong quá trình khởi động ứng dụng:
        /// - Trong constructor MainWindow sau khi khởi tạo UI
        /// - Trong App.xaml.cs trong quá trình thiết lập ứng dụng
        /// - Thông qua cấu hình dependency injection container
        /// </remarks>
        /// <exception cref="ArgumentNullException">Nên được throw khi frame là null</exception>
        void Initialize(Frame frame);

        /// <summary>
        /// Điều hướng đến loại trang được chỉ định nếu nó khác với trang hiện tại.
        /// </summary>
        /// <param name="pageType">
        /// Type của trang để điều hướng đến. Phải là một Page-derived type hợp lệ.
        /// Ví dụ bao gồm: typeof(LoginView), typeof(RegisterView), typeof(DashboardView)
        /// </param>
        /// <remarks>
        /// Implementation nên:
        /// - Xác minh rằng navigation service đã được khởi tạo
        /// - Kiểm tra xem loại trang đích có khác với trang hiện đang hiển thị không
        /// - Chỉ thực hiện điều hướng nếu các loại trang khác nhau
        /// - Xử lý thất bại điều hướng một cách nhẹ nhàng
        /// - Tùy chọn cung cấp tham số điều hướng hoặc quản lý trạng thái
        /// 
        /// Phương thức này hữu ích khi:
        /// - Loại trang được xác định tại runtime
        /// - Làm việc với reflection hoặc tải trang động
        /// - Xây dựng các tiện ích điều hướng generic
        /// 
        /// Để đảm bảo type safety tại compile-time, ưu tiên sử dụng NavigateTo&lt;T&gt;() khi có thể.
        /// </remarks>
        void NavigateTo(Type pageType);

        /// <summary>
        /// Điều hướng đến loại trang được chỉ định sử dụng tham số type generic để đảm bảo safety tại compile-time.
        /// Đây là phương thức điều hướng ưa thích khi loại trang được biết tại compile time.
        /// </summary>
        /// <typeparam name="T">
        /// Loại trang để điều hướng đến. Phải kế thừa từ class Page.
        /// Cung cấp kiểm tra type tại compile-time và hỗ trợ IntelliSense.
        /// </typeparam>
        /// <remarks>
        /// Implementation nên:
        /// - Gọi NavigateTo(typeof(T)) bên trong để có hành vi nhất quán
        /// - Cung cấp logic điều hướng tương tự như phiên bản không generic
        /// - Duy trì các đặc điểm hiệu suất tương tự
        /// 
        /// Lợi ích của phương thức này:
        /// - Type safety tại compile-time ngăn chặn lỗi loại trang không hợp lệ
        /// - Hỗ trợ IntelliSense hiển thị các loại trang có sẵn
        /// - Hỗ trợ refactoring tự động cập nhật các lời gọi phương thức
        /// - Code sạch hơn, dễ đọc hơn trong ViewModels và controllers
        /// - Giảm lỗi runtime từ lỗi đánh máy hoặc tên type không hợp lệ
        /// 
        /// Phương thức này được ưa thích khi:
        /// - Loại trang được biết tại compile time
        /// - Type safety là quan trọng
        /// - Làm việc trong các kịch bản strongly-typed
        /// - Xây dựng code có thể bảo trì, thân thiện với refactor
        /// </remarks>
        /// <example>
        /// <code>
        /// // Điều hướng đến trang đăng nhập
        /// navigationService.NavigateTo&lt;LoginView&gt;();
        /// 
        /// // Điều hướng đến dashboard sau đăng nhập thành công
        /// navigationService.NavigateTo&lt;DashboardView&gt;();
        /// 
        /// // Điều hướng đến trang đăng ký
        /// navigationService.NavigateTo&lt;RegisterView&gt;();
        /// </code>
        /// </example>
        void NavigateTo<T>() where T : Page;
    }
}