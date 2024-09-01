namespace takeout_tj.DTO
{
    public class MerchantDBDto
    {
        public int MerchantId { get; set; }
        public string Password { get; set; }
        public string MerchantName { get; set; }
        public string MerchantAddress { get; set; }
        public string Contact { get; set; }
        public string DishType { get; set; }
        public int TimeforOpenBusiness { get; set; }
        public int TimeforCloseBusiness { get; set; }
        public decimal Wallet { get; set; }
        public string WalletPassword { get; set; }
        public int CouponType { get; set; }

    }
}
