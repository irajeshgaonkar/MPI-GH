using System;
using HCA.Models.MuleSoft;

namespace HCA.Models
{
	public class IdentityFilter
	{
		public IdentityFilter()
		{
			Sources = new List<Source>();
			Emails = new List<string>();
			Addresses = new List<Address>();
			Names = new List<Name>();
			Ssns = new List<string>();
			Genders = new List<string>();
			DateOfBirths = new List<string>();
			PhoneNumbers = new List<PhoneNumber>();
		}

		public List<Source> Sources { get; set; }

		public List<string> Emails { get; set; }

		public List<Address> Addresses { get; set; }

		public List<Name> Names { get; set; }

		public List<string> Ssns { get; set; }

		public List<string> Genders { get; set; }

		public List<string> DateOfBirths { get; set; }

		public List<PhoneNumber> PhoneNumbers { get; set; }
	}

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

	public class Name
	{
		public Name(string first, string middle, string last, string suffix)
		{
			First = first;
			Middle = middle;
			Last = last;
			Suffix = suffix;
		}

		public string First { get; set; }

		public string Middle { get; set; }

		public string Last { get; set; }

		public string Suffix { get; set; }
	}

	public class PhoneNumber
	{
		public PhoneNumber(string number, string areaCode, string extension, string countryCode)
		{
			Number = number;
			AreaCode = areaCode;
			Extension = extension;
			CountryCode = countryCode;
		}

		public string Number { get; set; }

		public string AreaCode { get; set; }

		public string Extension { get; set; }

		public string CountryCode { get; set; }
	}
}

