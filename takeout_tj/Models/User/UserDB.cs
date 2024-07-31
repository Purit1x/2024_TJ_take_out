using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.User
{
	public class UserDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "User name is required.")]
		[StringLength(50, ErrorMessage = "The field UserName must be at most 15 characters long. ")]
		public string UserName { get; set; }

		[Required(ErrorMessage = "Phone number is required.")]
		[StringLength(11, MinimumLength = 11, ErrorMessage = "The field PhoneNumber must be exactly 10 digitss. ")]
		[RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number can only contain digits. ")]
		public string PhoneNumber {  get; set; }

		[Required(ErrorMessage = "User password is required. ")]
		[StringLength(20, MinimumLength = 6, ErrorMessage = "The password should be at least 6 characters and at most 20 characters long. ")]
		public string Password { get; set; }
	}
}
