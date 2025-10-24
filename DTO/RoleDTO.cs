using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SvendeApi.DTO;

public class RoleDTO
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; }
}

public class CreateRoleDTO
{
    [Required]
    [StringLength(50)]
    public string RoleName { get; set; }
}

public class UpdateRoleDTO
{
    [StringLength(50)]
    public string? RoleName { get; set; }
}