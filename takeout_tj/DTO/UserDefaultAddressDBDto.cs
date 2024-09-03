using System.ComponentModel.DataAnnotations;
using takeout_tj.Models.User;

namespace takeout_tj.DTO
{
    public class UserDefaultAddressDBDto
    {
        public int AddressId { get; set; }

        public int UserId { get; set; }
    }
}
