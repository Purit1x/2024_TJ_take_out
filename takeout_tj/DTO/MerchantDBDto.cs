namespace takeout_tj.DTO
{
    public class MerchantDBDto
    {
        public int MerchantId { get; set; }
        public string Password { get; set; }
        public string MerchantName { get; set; }
        public string MerchantAddress { get; set; }
        public string Contact { get; set; }
        public int CouponType { get; set; } = 0;
        public string DishType { get; set; }
        public DateTime TimeforOpenBusiness { get; set; }
        public DateTime TimeforCloseBusiness { get; set; }

    }
}
