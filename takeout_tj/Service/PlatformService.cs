using takeout_tj.Data;

namespace takeout_tj.Service
{
    public class PlatformService
    {
        ApplicationDbContext _context;
        public PlatformService(ApplicationDbContext context)
        {
            _context = context;
        }
        public int AssignId()
        {
            // 找到所有已存在的 ID  
            var usedIds = _context.Admins.Select(u => u.AdminId).ToList();

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
        public int AssignStationId()
        {
            // 找到所有已存在的 ID  
            var usedIds = _context.Stations.Select(u => u.StationId).ToList();

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
        public int AssignCouponId()
        {
            var usedIds = _context.Coupons.Select(u => u.CouponId).ToList();

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
    }
}
