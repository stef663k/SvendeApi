using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class UserRole
{
    public Guid UserId { get; set; }
    public UserModel? User { get; set; }
    public Guid RoleId { get; set; }
    public RoleModel? Role { get; set; }
}