using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.User
{
    public class UserDefaultAddressDB
    {
		[Required(ErrorMessage = "Address ID is required.")]
		public int AddressId { get; set; }

		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		public UserAddressDB UserAddressDB { get; set; }
		public UserDB UserDB { get; set; }
	}
}
