using Microsoft.UI.Xaml.Controls;
using System;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Service chịu trách nhiệm xử lý điều hướng giữa các trang trong ứng dụng.
    /// Triển khai interface INavigationService để cung cấp chức năng điều hướng.
    /// </summary>
    /// <remarks>
    /// Service này quản lý điều hướng trang thông qua WinUI Frame control và cung cấp:
    /// - Khởi tạo và quản lý Frame
    /// - Điều hướng type-safe đến các loại trang cụ thể
    /// - Phương thức điều hướng generic để cải thiện type safety
    /// - Ngăn chặn điều hướng không cần thiết đến cùng một trang
    /// - Tách biệt rõ ràng giữa logic điều hướng và các thành phần UI
    /// 
    /// Service phải được khởi tạo với một Frame trước khi có thể thực hiện các hoạt động điều hướng.
    /// </remarks>
    public class NavigationService : INavigationService
    {
        #region Private Fields

        /// <summary>
        /// WinUI Frame control được sử dụng cho điều hướng trang.
        /// Null cho đến khi phương thức Initialize được gọi.
        /// </summary>
        private Frame? _frame;

        #endregion

        #region Public Methods

        /// <summary>
        /// Khởi tạo navigation service với Frame control được chỉ định.
        /// Phương thức này phải được gọi trước khi có thể thực hiện bất kỳ hoạt động điều hướng nào.
        /// </summary>
        /// <param name="frame">
        /// WinUI Frame control sẽ được sử dụng cho điều hướng trang.
        /// Thường là frame chính của cửa sổ ứng dụng.
        /// </param>
        /// <remarks>
        /// Phương thức này nên được gọi một lần trong quá trình khởi động ứng dụng, thường là trong
        /// constructor MainWindow hoặc App.xaml.cs sau khi cửa sổ chính được tạo.
        /// 
        /// Ví dụ sử dụng:
        /// <code>
        /// var navigationService = new NavigationService();
        /// navigationService.Initialize(MainFrame);
        /// </code>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Được throw khi tham số frame là null</exception>
        public void Initialize(Frame frame)
        {
            _frame = frame ?? throw new ArgumentNullException(nameof(frame));
        }

        /// <summary>
        /// Điều hướng đến loại trang được chỉ định nếu nó khác với trang hiện tại.
        /// </summary>
        /// <param name="pageType">
        /// Type của trang để điều hướng đến. Phải kế thừa từ Page.
        /// Ví dụ phổ biến bao gồm: LoginView, RegisterView, DashboardView, v.v.
        /// </param>
        /// <remarks>
        /// Phương thức này thực hiện các kiểm tra sau:
        /// 1. Xác minh rằng Frame đã được khởi tạo
        /// 2. Kiểm tra xem loại trang đích có khác với trang hiện tại không
        /// 3. Chỉ điều hướng nếu các loại trang khác nhau (ngăn chặn điều hướng không cần thiết)
        /// 
        /// Điều hướng sẽ bị bỏ qua nếu:
        /// - Frame không được khởi tạo (null)
        /// - Trang hiện tại đã là loại được chỉ định
        /// 
        /// Phương thức này thường được sử dụng khi bạn có loại trang như một đối tượng Type runtime.
        /// Để đảm bảo type safety tại compile-time, hãy cân nhắc sử dụng phương thức NavigateTo&lt;T&gt;() generic thay thế.
        /// </remarks>
        /// <example>
        /// <code>
        /// // Điều hướng đến LoginView
        /// navigationService.NavigateTo(typeof(LoginView));
        /// 
        /// // Điều hướng đến RegisterView
        /// navigationService.NavigateTo(typeof(RegisterView));
        /// </code>
        /// </example>
        /// <seealso cref="NavigateTo{T}"/>
        public void NavigateTo(Type pageType)
        {
            if (_frame != null && _frame.Content?.GetType() != pageType)
            {
                _frame.Navigate(pageType);
            }
        }

        /// <summary>
        /// Điều hướng đến loại trang được chỉ định sử dụng tham số type generic để đảm bảo type safety tại compile-time.
        /// Đây là phương thức ưa thích cho điều hướng khi loại trang được biết tại compile time.
        /// </summary>
        /// <typeparam name="T">
        /// Loại trang để điều hướng đến. Phải kế thừa từ class Page.
        /// Ví dụ: LoginView, RegisterView, DashboardView, v.v.
        /// </typeparam>
        /// <remarks>
        /// Phương thức này cung cấp compile-time type safety và hỗ trợ IntelliSense so với
        /// phương thức NavigateTo(Type) không generic. Nó gọi NavigateTo(typeof(T)) bên trong.
        /// 
        /// Lợi ích của việc sử dụng phương thức này:
        /// - Kiểm tra type tại compile-time đảm bảo các loại trang hợp lệ
        /// - Hỗ trợ IntelliSense cho các loại trang có sẵn
        /// - Thân thiện với refactoring (việc đổi tên được xử lý tự động)
        /// - Code sạch hơn, dễ đọc hơn
        /// 
        /// Các quy tắc điều hướng tương tự áp dụng như phương thức không generic:
        /// - Frame phải được khởi tạo
        /// - Điều hướng chỉ xảy ra nếu trang đích khác với trang hiện tại
        /// </remarks>
        /// <example>
        /// <code>
        /// // Điều hướng type-safe đến LoginView
        /// navigationService.NavigateTo&lt;LoginView&gt;();
        /// 
        /// // Điều hướng type-safe đến RegisterView
        /// navigationService.NavigateTo&lt;RegisterView&gt;();
        /// 
        /// // Điều hướng type-safe đến DashboardView
        /// navigationService.NavigateTo&lt;DashboardView&gt;();
        /// </code>
        /// </example>
        /// <seealso cref="NavigateTo(Type)"/>
        public void NavigateTo<T>() where T : Page
        {
            NavigateTo(typeof(T));
        }

        #endregion
    }
}