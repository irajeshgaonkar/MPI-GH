namespace HCA.Models.MuleSoft;

/// <summary>
/// Address Model
/// </summary>
public class Address
{
		public Address(string line1, string line2, string city, string state, string postalCode)
		{
			Line1 = line1;
			Line2 = line2;
			City = city;
			State = state;
			PostalCode = postalCode;
		}

		public string Line1 { get; set; }

		public string Line2 { get; set; }

		public string City { get; set; }

		public string State { get; set; }

		public string PostalCode { get; set; }
	}

