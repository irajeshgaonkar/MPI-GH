using System.ComponentModel.DataAnnotations;

namespace HCA.Api.Dto;

public class UserCredentials
{
    [Required]
    [DataType(DataType.EmailAddress)]
    [StringLength(255)]
    public string Email { get; set; }

    [Required]
    [StringLength(32)]
    public string Password { get; set; }
}

