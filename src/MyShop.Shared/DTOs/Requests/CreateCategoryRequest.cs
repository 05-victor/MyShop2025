using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Requests;

public class CreateCategoryRequest
{
    public required string Name { get; set; }
    public string? Description { get; set; }
}
