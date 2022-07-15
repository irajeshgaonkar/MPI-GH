using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HCA.Data.Entities;

[Table("roles")]
public class RoleEntity
{
    [Key]
    [Column("role_id")]
    public int Id { get; set; }

    [Column("name")]
    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    public List<UserRoleEntity> UserRoles { get; set; }
}

