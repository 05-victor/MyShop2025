using System.Threading.Tasks;
using MyShop.Shared.DTOs;

namespace MyShop.Client.Services
{
    /// <summary>
    /// Interface định nghĩa các hoạt động service xác thực để quản lý người dùng.
    /// Cung cấp các hợp đồng cho đăng nhập, đăng ký và lấy thông tin người dùng.
    /// </summary>
    /// <remarks>
    /// Interface này trừu tượng hóa các hoạt động xác thực, cho phép:
    /// - Dependency injection và kiểm thử thông qua các implementation giả lập
    /// - API xác thực nhất quán trên các implementation khác nhau
    /// - Dễ dàng chuyển đổi giữa implementation phát triển/giả lập và production
    /// - Tách biệt rõ ràng các mối quan tâm trong kiến trúc ứng dụng
    /// </remarks>
    public interface IAuthService
    {
        /// <summary>
        /// Xác thực người dùng với thông tin đăng nhập của họ một cách bất đồng bộ.
        /// </summary>
        /// <param name="usernameOrEmail">
        /// Username hoặc địa chỉ email của người dùng được sử dụng để xác thực.
        /// Service nên xử lý cả định dạng username và email.
        /// </param>
        /// <param name="password">
        /// Mật khẩu của người dùng để xác thực.
        /// Nên được validate theo các yêu cầu bảo mật tối thiểu.
        /// </param>
        /// <returns>
        /// Một task đại diện cho hoạt động xác thực bất đồng bộ.
        /// Kết quả task chứa một <see cref="AuthResponse"/> với:
        /// - Success: true nếu xác thực thành công, false nếu không
        /// - Message: thông báo mô tả về kết quả
        /// - Token: token xác thực nếu thành công (cho ủy quyền API)
        /// - User: thông tin người dùng nếu thành công
        /// </returns>
        /// <remarks>
        /// Implementation nên:
        /// - Validate các tham số đầu vào
        /// - Xử lý cả xác thực username và email
        /// - Trả về thông báo lỗi phù hợp cho các kịch bản thất bại khác nhau
        /// - Bao gồm token xác thực cho các API call tiếp theo
        /// - Xử lý lỗi mạng và server một cách nhẹ nhàng
        /// </remarks>
        Task<AuthResponse> LoginAsync(string usernameOrEmail, string password);

        /// <summary>
        /// Đăng ký người dùng mới với thông tin được cung cấp một cách bất đồng bộ.
        /// </summary>
        /// <param name="username">
        /// Username mong muốn cho tài khoản người dùng mới.
        /// Nên là duy nhất trong hệ thống.
        /// </param>
        /// <param name="email">
        /// Địa chỉ email cho tài khoản người dùng mới.
        /// Nên là duy nhất và ở định dạng email hợp lệ.
        /// </param>
        /// <param name="password">
        /// Mật khẩu cho tài khoản người dùng mới.
        /// Nên đáp ứng các yêu cầu bảo mật (độ dài tối thiểu, độ phức tạp, v.v.)
        /// </param>
        /// <returns>
        /// Một task đại diện cho hoạt động đăng ký bất đồng bộ.
        /// Kết quả task chứa một <see cref="AuthResponse"/> với:
        /// - Success: true nếu đăng ký thành công, false nếu không
        /// - Message: thông báo mô tả về kết quả
        /// - Token: token xác thực nếu thành công (tự động đăng nhập sau đăng ký)
        /// - User: thông tin người dùng mới được tạo nếu thành công
        /// </returns>
        /// <remarks>
        /// Implementation nên:
        /// - Validate tất cả các tham số đầu vào
        /// - Kiểm tra username và email trùng lặp
        /// - Thực thi các yêu cầu bảo mật mật khẩu
        /// - Trả về thông báo lỗi phù hợp cho các thất bại validation
        /// - Tùy chọn tự động xác thực người dùng sau đăng ký thành công
        /// - Xử lý lỗi mạng và server một cách nhẹ nhàng
        /// </remarks>
        Task<AuthResponse> RegisterAsync(string username, string email, string password);

        /// <summary>
        /// Lấy thông tin người dùng được xác thực hiện tại một cách bất đồng bộ.
        /// </summary>
        /// <returns>
        /// Một task đại diện cho hoạt động bất đồng bộ.
        /// Kết quả task chứa một <see cref="UserDto"/> với thông tin người dùng hiện tại
        /// nếu người dùng được xác thực và request thành công, hoặc null nếu không.
        /// </returns>
        /// <remarks>
        /// Implementation nên:
        /// - Sử dụng token xác thực hiện tại để xác minh danh tính người dùng
        /// - Trả về thông tin người dùng đầy đủ cho người dùng được xác thực
        /// - Trả về null cho người dùng chưa xác thực hoặc khi có lỗi
        /// - Xử lý token hết hạn hoặc không hợp lệ một cách phù hợp
        /// - Xử lý lỗi mạng và server bằng cách trả về null
        /// 
        /// Phương thức này thường được sử dụng để:
        /// - Xác minh trạng thái xác thực hiện tại
        /// - Làm mới thông tin người dùng trong ứng dụng
        /// - Kiểm tra xem token xác thực có còn hợp lệ không
        /// </remarks>
        Task<UserDto?> GetCurrentUserAsync();
    }
}