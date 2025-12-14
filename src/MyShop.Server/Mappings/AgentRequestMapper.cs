using MyShop.Data.Entities;
using MyShop.Shared.DTOs.Responses;

namespace MyShop.Server.Mappings;
public class AgentRequestMapper
{
    public static AgentRequestResponse ToAgentRequestResponse(AgentRequest request)
    {
        return new AgentRequestResponse
        {
            Id = request.Id,
            UserId = request.UserId,
            FullName = request.User?.Profile?.FullName ?? request.User?.Username ?? "",
            Email = request.User?.Email ?? "",
            PhoneNumber = request.User?.Profile?.PhoneNumber ?? "",
            AvatarUrl = request.User?.Profile?.Avatar,
            RequestedAt = request.RequestedAt,
            Status = request.Status,
            ReviewedBy = request.ReviewedBy?.ToString(),
            ReviewedAt = request.ReviewedAt,
            Notes = request.Notes,
            Experience = request.Experience ?? "",
            Reason = request.Reason ?? "",
            Address = request.User?.Profile?.Address,
        };
    }
}

