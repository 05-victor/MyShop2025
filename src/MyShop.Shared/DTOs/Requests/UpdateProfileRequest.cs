using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Shared.DTOs.Requests;
public class UpdateProfileRequest
{
    public string? Avatar { get; set; }

    [MinLength(2, ErrorMessage = "Full name must be at least 2 characters long.")]
    [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters.")]
    public string? FullName { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; }

    [MaxLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
    public string? Address { get; set; }
}
