using System.ComponentModel.DataAnnotations;

namespace takeout_tj.Models.User
{
	public class MembershipDB
	{
		[Required(ErrorMessage = "User ID is required.")]
		public int UserId { get; set; }

		[Required(ErrorMessage = "User ID is required.")]
		[Range(1,5,ErrorMessage = "Level must be between 1 and 5")]
		public int Level { get; set; }

		[Required(ErrorMessage = "Points is required.")]
		[Range(0,int.MaxValue,ErrorMessage = "Points must be non-negative. ")]
		public int Points { get; set; }

		public UserDB UserDB { get; set; }  // 用于定义外键的导航属性
	}
}
