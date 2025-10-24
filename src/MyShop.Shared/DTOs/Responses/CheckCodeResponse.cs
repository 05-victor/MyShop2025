using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Responses;
public class CheckCodeResponse
{
    public bool Success { get; set; }

    public bool Valid { get; set; }
    public string? Reason { get; set; }

    //public int? Code { get; set; }
}
