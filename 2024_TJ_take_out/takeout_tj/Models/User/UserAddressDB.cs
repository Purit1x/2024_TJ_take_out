using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.User
{
	public class UserAddressDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "User address is needed. ")]
		[StringLength(255, ErrorMessage = "The field UserAddress must be at most 255 characters long. ")]
		public string UserAddress { get; set; }

		public UserDB UserDB { get; set; }  // 导航属性，用于直接找到外键引用的类
	}
}
