using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

[Table("user_roles")]
public class UserRoleEntity
{
    [Column("role_id")]
    public int RoleId { get; set; }

    [Column("user_id")]
    public int UserId { get; set; }

    public RoleEntity Role { get; set; }

    public UserEntity User { get; set; }
}

