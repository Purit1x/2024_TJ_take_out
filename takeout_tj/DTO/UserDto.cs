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
}
