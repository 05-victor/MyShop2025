using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Responses;

public class ProductResponse
{
    public Guid Id { get; set; }
    public string? SKU { get; set; }
    public string? Name { get; set; }
    public string? Manufacturer { get; set; }
    public string? DeviceType { get; set; }
    public int? ImportPrice { get; set; }
    public int? SellingPrice { get; set; }
    public int? Quantity { get; set; }
    public double? CommissionRate { get; set; }
    public string? Status { get; set; } = "AVAILABLE";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CategoryName { get; set; }
}
