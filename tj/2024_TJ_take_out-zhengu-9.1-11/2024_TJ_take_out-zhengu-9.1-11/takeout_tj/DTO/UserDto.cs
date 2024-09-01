using System.ComponentModel.DataAnnotations;

namespace takeout_tj.DTO
{
    public class UserDto 
    {
        public int UserId { get; set; }

        public string UserName { get; set; }
        public string PhoneNumber { get; set; }
        public string Password { get; set; }
        public decimal Wallet { get; set; }
        public string WalletPassword { get; set; }
    }
    public class AddressDto
    {
        public int UserId { get; set; }
        public int AddressId { get; set; }
        public string Address { get; set; }
        public string HouseNumber { get; set; }
        public string ContactName { get; set; }
        public string PhoneNumber { get; set; }
    }
}
