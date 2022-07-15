using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

[Table("users")]
public class UserEntity : BaseEntity
{
    [Key]
    [Column("user_id")]
    public int UserId { get; set; }

    [Column("email")]
    [Required]
    [MaxLength(255)]
    public string Email { get; set; }

    [Column("password")]
    [Required]
    [MaxLength(1024)]
    public string Password { get; set; }

    [Column("first_name")]
    [Required]
    [MaxLength(255)]
    public string FirstName { get; set; }

    [Column("middle_name")]
    [MaxLength(255)]
    public string? MiddleName { get; set; }

    [Column("last_name")]
    [Required]
    [MaxLength(255)]
    public string LastName { get; set; }

    public List<UserRoleEntity> UserRoles { get; set; }
}

