using System.ComponentModel.DataAnnotations.Schema;
namespace HCA.Data.Entities;

[Table("client_identity_address_communication")]
public class ClientIdentityAddressCommunicationEntity
{
    [Column("client_identity_address_id")]
    public int ClientIdentityAddressId { get; set; }

    [Column("client_identity_communication_id")]
    public int ClientIdentityCommunicationId { get; set; }

    public ClientIdentityAddressEntity Address { get; set; }

    public ClientIdentityCommunicationEntity Communication { get; set; }
}
