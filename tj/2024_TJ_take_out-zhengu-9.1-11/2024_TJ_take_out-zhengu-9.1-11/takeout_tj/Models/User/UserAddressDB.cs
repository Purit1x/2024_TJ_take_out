using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.Platform;

namespace takeout_tj.Models.User
{
    public class UserAddressDB
    {
        [Required(ErrorMessage = "Address ID is required.")]
        public int AddressId { get; set; }

        [Required(ErrorMessage = "User ID is required.")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "User address is needed. ")]
        [StringLength(255, ErrorMessage = "The field UserAddress must be at most 255 characters long. ")]
        public string UserAddress { get; set; }

        [StringLength(50, ErrorMessage = "The field HouseNumber must be at most 50 characters long.")]
        public string HouseNumber { get; set; } // 门牌号

        [Required(ErrorMessage = "Contact Name is required.")]
        [StringLength(100, ErrorMessage = "The field ContactName must be at most 100 characters long.")]
        public string ContactName { get; set; } // 联系人姓名

        [Required(ErrorMessage = "Phone number is required.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "The field PhoneNumber must be exactly 11 digits.")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number can only contain digits.")]
        public string PhoneNumber { get; set; } // 联系电话

        public UserDB UserDB { get; set; }  // 导航属性，用于直接找到外键引用的类
        public ICollection<OrderDB> OrderDBs { get; set; }
        public UserDefaultAddressDB UserDefaultAddressDB { get; set; }  
    }
}
