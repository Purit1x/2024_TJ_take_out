using System;  
using System.Linq;  
using System.Threading.Tasks;  
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.Models.Platform;
using takeout_tj.Models.User;

public class OrderCleanupService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderCleanupService> _logger; // 添加日志记录字段
    public OrderCleanupService(ApplicationDbContext context, ILogger<OrderCleanupService> logger)
    {
        _context = context;
        _logger = logger;
    }
    /*
    public async Task RemoveExpiredOrders()
    {
        var now = DateTime.UtcNow; // 确保使用UTC时间  

        // 设置过期阈值  
        var expirationThreshold = now.AddMinutes(-15);

        var expiredOrders = await _context.Orders
            .Where(order => order.State == 0 &&
                            order.OrderTimestamp < expirationThreshold)
            .ToListAsync();

        _logger.LogInformation($"Found {expiredOrders.Count} expired orders.");

        if (expiredOrders.Any())
        {
            _context.Orders.RemoveRange(expiredOrders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Expired orders removed.");
        }
        else
        {
            _logger.LogInformation("No expired orders to remove.");
        }
    }*/
    public async Task RemoveExpiredOrders()
    {
        var now = DateTime.UtcNow; // 确保使用UTC时间  
        var expirationThreshold = now.AddMinutes(-15); // 设置过期阈值  

        var expiredOrders = await _context.Orders
            .Where(order => order.State == 0 && order.OrderTimestamp < expirationThreshold)
            .ToListAsync();

        //_logger.LogInformation($"Found {expiredOrders.Count} expired orders.");

        foreach (var order in expiredOrders)
        {
            var orderDishes=_context.OrderDishes.Where(od=>od.OrderId== order.OrderId).ToList();
            await RestoreDishInventory(orderDishes); // 恢复菜品库存  
        }

        if (expiredOrders.Any())
        {
            _context.Orders.RemoveRange(expiredOrders);
            await _context.SaveChangesAsync();
            //_logger.LogInformation("Expired orders removed and dish inventories restored.");
        }
        else
        {
            //_logger.LogInformation("No expired orders to remove.");
        }
    }

    private async Task RestoreDishInventory(List<OrderDishDB> orderDishes)
    {
        foreach (var orderDish in orderDishes)
        {
            // 根据 DishId 查询菜品  
            var dish = await _context.Dishes.FirstOrDefaultAsync(d => d.DishId == orderDish.DishId);
            if (dish != null)
            {
                // 恢复库存  
                dish.DishInventory += orderDish.DishNum; // 增加菜品库存  
                _context.Dishes.Update(dish); // 确保将更新的菜品存回上下文  
            }
        }
        await _context.SaveChangesAsync(); // 保存所有库存更改  
    }
    public async Task RemovePaidOrders()  //清除没有被骑手接单的订单
    {
        var now = DateTime.UtcNow; // 确保使用UTC时间  
        var expirationThreshold = now.AddMinutes(-30); // 设置过期阈值  

        var expiredOrders = await _context.Orders
            .Where(order => order.State == 1 && order.OrderTimestamp < expirationThreshold)
            .ToListAsync();

        //_logger.LogInformation($"Found {expiredOrders.Count} expired orders.");

        foreach (var order in expiredOrders)
        {
            var orderDishes = _context.OrderDishes.Where(od => od.OrderId == order.OrderId).ToList();
            await RestoreDishInventory(orderDishes); // 恢复菜品库存  
            var orderCoupon = _context.OrderCoupons.FirstOrDefault(m => m.OrderId == order.OrderId);
            if (orderCoupon != null)
            {
                var userCoupon = _context.UserCoupons.FirstOrDefault(m => m.UserId == orderCoupon.UserId && m.CouponId == orderCoupon.CouponId && m.ExpirationDate == orderCoupon.ExpirationDate);
                if (userCoupon != null)
                {
                    userCoupon.AmountOwned++;
                  
                }
                else
                {
                    UserCouponDB newCoupon = new UserCouponDB
                    {
                        CouponId = orderCoupon.CouponId,
                        UserId = orderCoupon.UserId,
                        ExpirationDate = orderCoupon.ExpirationDate,
                        AmountOwned = 1
                    };
                    _context.UserCoupons.Add(newCoupon);
              
                }
            }
            var orderUser = _context.OrderUsers.FirstOrDefault(ou => ou.OrderId == order.OrderId);
            if (orderUser != null)
            {
                var user = _context.Users.FirstOrDefault(u=>u.UserId==orderUser.UserId);
                if (user != null)
                {
                    user.Wallet += order.Price;
                    _logger.LogInformation($"退还用户钱包: 用户ID = {user.UserId}, 金额 = {order.Price}");
                }
            }
        }
        if (expiredOrders.Any())
        {
            _context.Orders.RemoveRange(expiredOrders);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Expired orders removed and dish inventories restored.");
        }
        else
        {
            _logger.LogInformation("No expired orders to remove.");
        }
    }
}