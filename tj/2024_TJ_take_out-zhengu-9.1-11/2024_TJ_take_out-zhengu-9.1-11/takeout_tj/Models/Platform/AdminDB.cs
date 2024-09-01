using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.Platform
{
	public class AdminDB
	{
		[Required]
		public int AdminId { get; set; }

		[Required(ErrorMessage = "User password is required. ")]
		[StringLength(20, MinimumLength = 6, ErrorMessage = "The password should be at least 6 characters and at most 20 characters long. ")]
		public string Password { get; set; }
	}
}
