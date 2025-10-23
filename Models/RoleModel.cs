using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.Models;

public class RoleModel
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }
    public Collection<UserRole> UserRoles { get; set; }
}
public static class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Moderator = "Moderator";
}