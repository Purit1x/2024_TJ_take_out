using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;

namespace takeout_tj.Service
{
    public class CouponService:IDisposable
    {
        private readonly ApplicationDbContext _context; // 替换为您的 DbContext  
        private Timer _timer;

        public CouponService(ApplicationDbContext context)
        {
            _context = context;
            // 设置定时器每隔一小时检查一次（3600000毫秒）  
            _timer = new Timer(CheckExpiredCoupons, null, TimeSpan.Zero, TimeSpan.FromMinutes(1));
        }

        private async void CheckExpiredCoupons(object state)
        {
            var now = DateTime.UtcNow; // 获取当前时间  
            var expiredCoupons = await _context.UserCoupons
                .Where(c => c.ExpirationDate < now)
                .ToListAsync();

            if (expiredCoupons.Any())
            {
                _context.UserCoupons.RemoveRange(expiredCoupons); // 删除过期的优惠券  
                await _context.SaveChangesAsync(); // 保存更改  
            }
        }
        public void Dispose()
        {
            _timer?.Change(Timeout.Infinite, 0);
            _timer?.Dispose();
        }
    }
}
