using MyShop.Shared.DTOs.Requests;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Services.Interfaces;

/// <summary>
/// Interface cho service quản lý quyền hạn của user.
/// </summary>
public interface IUserAuthorityService
{
    /// <summary>
    /// Lấy danh sách quyền hạn hiệu lực của user (sau khi trừ đi quyền bị loại bỏ).
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <returns>Danh sách tên quyền hạn hiệu lực</returns>
    Task<IEnumerable<string>> GetEffectiveAuthoritiesAsync(Guid userId);

    /// <summary>
    /// Lấy thông tin chi tiết về quyền hạn hiệu lực của user.
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <returns>Response chứa thông tin đầy đủ về quyền hạn</returns>
    Task<EffectiveAuthoritiesResponse?> GetEffectiveAuthoritiesDetailAsync(Guid userId);

    /// <summary>
    /// Kiểm tra xem user có quyền hạn cụ thể hay không.
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="authorityName">Tên quyền hạn cần kiểm tra</param>
    /// <returns>Response chứa kết quả kiểm tra</returns>
    Task<CheckAuthorityResponse> HasAuthorityAsync(Guid userId, string authorityName);

    /// <summary>
    /// Thêm quyền hạn vào danh sách loại bỏ của user.
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="request">Thông tin quyền hạn cần loại bỏ</param>
    /// <returns>Response chứa thông tin quyền hạn đã loại bỏ</returns>
    Task<RemovedAuthorityResponse> AddRemovedAuthorityAsync(Guid userId, AddRemovedAuthorityRequest request);

    /// <summary>
    /// Xóa quyền hạn khỏi danh sách loại bỏ (khôi phục quyền cho user).
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <param name="authorityName">Tên quyền hạn cần khôi phục</param>
    /// <returns>True nếu xóa thành công, false nếu không tìm thấy</returns>
    Task<bool> RemoveRemovedAuthorityAsync(Guid userId, string authorityName);

    /// <summary>
    /// Lấy danh sách tất cả quyền hạn bị loại bỏ của user.
    /// </summary>
    /// <param name="userId">ID của user</param>
    /// <returns>Danh sách quyền hạn bị loại bỏ</returns>
    Task<IEnumerable<RemovedAuthorityResponse>> GetRemovedAuthoritiesAsync(Guid userId);
}
