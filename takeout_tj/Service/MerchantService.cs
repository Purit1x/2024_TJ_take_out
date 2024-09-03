using takeout_tj.Data;

namespace takeout_tj.Service
{
    public class MerchantService
    {
        ApplicationDbContext _context;
        public MerchantService(ApplicationDbContext context)
        {
            _context=context;
        }
        public int AssignId()
        {
            // 找到所有已存在的 ID  
            var usedIds = _context.Merchants.Select(u => u.MerchantId).ToList();

            // 从 1 开始找到一个未使用的 ID  
            for (int id = 1; id <= usedIds.Count + 1; id++)
            {
                if (!usedIds.Contains(id))
                {
                    return id; // 返回第一个未使用的 ID  
                }
            }
            return -1;
        }
        public int AssignDishId()
        {
            var dishIds = _context.Dishes.Select(u => u.DishId).ToList();

            for (int id = 1; id <= dishIds.Count + 1; id++)
            {
                if (!dishIds.Contains(id))
                {
                    return id; 
                }
            }
            return -1;
        }

        public int AssignSpecialOfferId()
        {
            var offerIds = _context.SpecialOffers.Select(u => u.OfferId).ToList();

            for (int id = 1; id <= offerIds.Count + 1; id++)
            {
                if (!offerIds.Contains(id))
                {
                    return id;
                }
            }
            return -1;
        }
    }
}
