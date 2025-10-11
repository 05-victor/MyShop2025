using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.Data.Entities
{
    public class RoleAuthorities
    {
        public string RoleName { get; set; } = string.Empty;
        public Role Role { get; set; } = null!;
        public string AuthorityName { get; set; } = string.Empty;
        public Authority Authority { get; set; } = null!;
    }
}
