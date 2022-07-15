namespace HCA.Models
{
    public class UserTokenModel
	{
		public string Token { get; set; }

		public long Expiration { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string MiddleName { get; set; }

		public UserRoleModel Role { get; set; }
	}
}

