using takeout_tj.Data;

namespace takeout_tj.Service
{
    public class UserService
    {
        ApplicationDbContext _context;
        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }
        public int AssignId()
        {
            // 找到所有已存在的 ID  
            var usedIds = _context.Users.Select(u => u.UserId).ToList();

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
        public int AssignCartId()
        {
            // 找到所有已存在的 ShoppingCartId  
            var usedCartIds = _context.ShoppingCarts.Select(c => c.ShoppingCartId).ToList();

            // 从 1 开始找到一个未使用的 ID  
            for (int id = 1; id <= usedCartIds.Count + 1; id++)
            {
                if (!usedCartIds.Contains(id))
                {
                    return id; // 返回第一个未使用的 ID  
                }
            }
            return -1;
        }
        public int AssignAddressId()
        {
            var usedAddressIds = _context.UserAddresses.Select(c => c.AddressId).ToList();

            // 从 1 开始找到一个未使用的 ID  
            for (int id = 1; id <= usedAddressIds.Count + 1; id++)
            {
                if (!usedAddressIds.Contains(id))
                {
                    return id; // 返回第一个未使用的 ID  
                }
            }
            return -1;
        }
        public int AssignCouponPurchaseId()
        {
            var usedCouponPurchaseIds = _context.CouponPurchases.Select(c => c.CouponPurchaseId).ToList();

            // 从 1 开始找到一个未使用的 ID  
            for (int id = 1; id <= usedCouponPurchaseIds.Count + 1; id++)
            {
                if (!usedCouponPurchaseIds.Contains(id))
                {
                    return id; // 返回第一个未使用的 ID  
                }
            }
            return -1;
        }
        public int AssignOrderId()
        {
            var usedOrderIds = _context.Orders.Select(c => c.OrderId).ToList();

            // 从 1 开始找到一个未使用的 ID  
            for (int id = 1; id <= usedOrderIds.Count + 1; id++)
            {
                if (!usedOrderIds.Contains(id))
                {
                    return id; // 返回第一个未使用的 ID  
                }
            }
            return -1;
        }
    }
}
