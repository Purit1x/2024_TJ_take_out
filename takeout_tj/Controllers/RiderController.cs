using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Rider;
using takeout_tj.Models.Platform;
using takeout_tj.Service;
using Microsoft.VisualBasic;

namespace takeout_tj.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RiderController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly RiderService _riderService;
        public RiderController(ApplicationDbContext context)
        {
            _context = context;
            _riderService = new RiderService(_context);
        }
        [HttpPost]
        [Route("register")]
        public IActionResult AddRider([FromBody] RiderDBDto dto)
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                RiderDB rider = new RiderDB()
                {
                    RiderId = _riderService.AssignId(),
                    RiderName = dto.RiderName,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber,
                    Wallet = 0.00m, // 初始化钱包
                    WalletPassword = dto.WalletPassword,
                };
                _context.Riders.Add(rider);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = rider.RiderId, msg = "注册成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }

            }
            catch (Exception ex)
            {
                tran.Rollback();    //多表添加才用到

                return StatusCode(20000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("login")] //登录
        public IActionResult Login([FromBody] RiderDBDto rider)
        {
            try
            {
                RiderDB _rider = new RiderDB()
                {
                    RiderId = rider.RiderId,
                    Password = rider.Password
                };
                // 条件查询
                var result1 = _context.Riders.Where(n => n.RiderId == _rider.RiderId && n.Password == _rider.Password).ToList();

                if (result1?.Count > 0)
                {
                    return StatusCode(200, new { msg = "ok" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "密码错误" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message.ToString() });
            }
        }
        [HttpGet]
        [Route("riderSearch")]  //查询个人信息
        public IActionResult GetRiderInfo(int riderId)
        {
            try
            {
                var rider = _context.Riders.FirstOrDefault(m => m.RiderId == riderId);

                if (rider == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "骑手未找到" });
                }

                var riderInfo = new
                {
                    rider.RiderId,
                    rider.RiderName,
                    rider.PhoneNumber,
                    rider.Password,
                    rider.Wallet,
                    rider.WalletPassword,
                };

                return Ok(new { data = riderInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPut]
        [Route("riderEdit")]  //编辑个人信息
        public IActionResult EditMerchant([FromBody] RiderDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                // 查找商家
                var rider = _context.Riders.FirstOrDefault(u => u.RiderId == dto.RiderId);
                if (rider == null)
                {
                    return NotFound(new { errorCode = 404, msg = "骑手未找到" });
                }
                // 更新商家信息  
                rider.RiderName = dto.RiderName;
                rider.PhoneNumber = dto.PhoneNumber;
                rider.Password = dto.Password;
                rider.Wallet = dto.Wallet;
                rider.WalletPassword = dto.WalletPassword;

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { msg = "用户信息更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "更新失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 30000, msg = $"更新异常: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("recharge")]
        public IActionResult WalletRecharge(int riderId, int addMoney)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                var rider = _context.Riders.FirstOrDefault(u => u.RiderId == riderId);
                if (rider == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                if (addMoney == 0)
                {
                    tran.Commit();
                    return Ok(new { data = rider.Wallet, msg = "用户信息更新成功" });
                }
                rider.Wallet = rider.Wallet + addMoney;
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data = rider.Wallet, msg = "用户信息更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "充值失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 30000, msg = $"充值异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("StIdSearch")]
        public IActionResult GetStationIdByRiderId(int riderId)
        {
            try
            {
                var riderStation = _context.RiderStations.FirstOrDefault(u => u.RiderId == riderId);

                if (riderStation == null)
                {
                    return NotFound(new { errorCode = 404, msg = "未分配" });
                }
                return Ok(new { data = riderStation.StationId, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpGet]
        [Route("getReceivedOrders")]
        public async Task<IActionResult> getReceivedOrders(int riderId)
        {
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    var orderIds = await _context.OrderRiders  // 指定骑手的所有订单
                        .Where(or => or.RiderId == riderId)
                        .Select(or => or.OrderId).ToListAsync();
                    var orderIdsWithStateOne = await _context.Orders  // 指定骑手的订单中正在派送的
                        .Where(o => orderIds.Contains(o.OrderId) && o.State == 2)
                        .ToListAsync();
                    if (orderIdsWithStateOne == null)
                        return Ok(new { data = 20000, msg = "该骑手尚未接单" });

                    return Ok(new { data = orderIdsWithStateOne, msg = "获取成功" });
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, MsgBoxResult = ex.Message });
                }
            }
        }
        [HttpGet]
        [Route("getPaidOrders")]
        public IActionResult getPaidOrders(int riderId)
        {
            try
            {
                // 获取rider对应的StationId  
                var riderStation = _context.RiderStations
                    .FirstOrDefault(rs => rs.RiderId == riderId);
                if (riderStation == null)
                {
                    return NotFound(new { errorCode = 40001, msg = "未找到与该骑手相关的站点" });
                }

                var riderStationId = riderStation.StationId;

                // 查询状态为1的订单，并且与Merchant的StationId相同的订单  
                var orders = _context.Orders
                    .Where(o => o.State == 1)
                    .ToList();

                var results = new List<object>();

                foreach (var order in orders)
                {
                    var orderDishes = _context.OrderDishes.Where(od => od.OrderId == order.OrderId).ToList();
                    // 查找Dish对应的Merchant  
                    var merchantStation = _context.MerchantStations
                        .FirstOrDefault(m => m.MerchantId == orderDishes[0].MerchantId);

                    if (merchantStation != null)
                    {
                        // 将 Merchant 的 StationId 和 order 相关的信息保存  
                        var stationId = merchantStation.StationId;

                        // 检查 StationId 是否与骑手的 StationId 相同  
                        if (stationId == riderStationId)
                        {
                            results.Add(new
                            {
                                order
                            });
                        }
                    }
                }

                if (results.Count > 0)
                {
                    return Ok(new { data = results, msg = "查找成功" }); // 返回找到的订单  
                }
                else
                {
                    return Ok(new { data = 40000, msg = "未找到相关订单" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("receiveOrder")]  // 骑手接单
        public async Task<IActionResult> ReceiveOrder([FromBody] RiderReceiveOrder riderReceiveOrderDto)
        {
            using (var tran = _context.Database.BeginTransaction())
            {
                try
                {
                    var orderRiderToUpdate = await _context.OrderRiders
                        .FirstOrDefaultAsync(or => or.OrderId == riderReceiveOrderDto.OrderId);  // 获取指定元组，将RiderId填入
                    if (orderRiderToUpdate == null)
                    {
                        tran.Rollback();
                        return NotFound(new { errorCode = 404, msg = "指定订单不存在" });
                    }
                    orderRiderToUpdate.RiderId = riderReceiveOrderDto.RiderId;

                    /*var result1 = _context.SaveChanges();
                    if (result1 <= 0)
                    {
                        tran.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, msg = "更新订单骑手失败" });
                    }*/

                    var orderToUpdate = await _context.Orders.FirstOrDefaultAsync(o => o.OrderId == riderReceiveOrderDto.OrderId);
                    if (orderToUpdate == null)
                    {
                        tran.Rollback();
                        return NotFound(new { errorCode = 404, msg = "指定订单不存在" });
                    }
                    orderToUpdate.State = 2;
                    var result2 = _context.SaveChanges();
                    if (result2 <= 0)
                    {
                        tran.Rollback();
                        return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, msg = "更新订单状态失败" });
                    }

                    tran.Commit();
                    return Ok(new { data = orderToUpdate });  // 返回更新的订单
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return BadRequest(new { errorCode = 400, msg = $"接单异常：{ex.Message}" });
                }
            }
        }
        [HttpGet]
        [Route("getRiderPrice")]
        public async Task<IActionResult> GetRiderPrice(int orderId)
        {
            try
            {
                var orderRider = await _context.OrderRiders.FirstOrDefaultAsync(or => or.OrderId == orderId);
                if (orderRider == null)
                {
                    return NotFound(new { errorCode = 404, msg = "指定订单不存在" });
                }
                return Ok(new { data = orderRider.RiderPrice, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, msg = "获取失败" });
            }
        }
		[HttpGet]
		[Route("getRiderInOrder")]//按照指定顺序显示骑手列表
		public IActionResult GetRiderInOrder()
		{
			// 查询状态为已送达的订单
			var deliveredOrderIds = _context.Set<OrderDB>()
				.Where(o => o.State == 3)  // 假设3代表订单已送达
				.Select(o => o.OrderId)
				.ToList(); // 确保转换为列表，以便在后续查询中使用

			// 通过中间表关联订单和骑手
			var orderRiders = _context.Set<OrderRiderDB>()
				.Where(or => deliveredOrderIds.Contains(or.OrderId))
				.ToList(); // 确保转换为列表以进行进一步操作

			// 统计每位骑手的已送达订单数和评分
			var ridersWithDeliveredOrders = orderRiders
				.GroupBy(o => o.RiderId)
				.Select(g => new
				{
					RiderId = g.Key,
					// 计算每位骑手的平均评分
					AverageRiderRating = g.Average(o => o.RiderPrice), // 假设用 RiderPrice 代替评分
					DeliveredOrders = g.Count()
				})
				.OrderByDescending(r => r.AverageRiderRating)
				.ThenByDescending(r => r.DeliveredOrders)
				.ToList();

			// 获取骑手详细信息
			var riderDetails = _context.Set<RiderDB>()
				.Where(r => ridersWithDeliveredOrders.Select(ro => ro.RiderId).Contains(r.RiderId))
				.ToList();

			// 合并统计信息和详细信息
			var result = ridersWithDeliveredOrders
				.Join(riderDetails,
					  stat => stat.RiderId,
					  detail => detail.RiderId,
					  (stat, detail) => new
					  {
						  detail.RiderId,
						  detail.RiderName,
						  detail.PhoneNumber,

						  stat.DeliveredOrders,
						  stat.AverageRiderRating
					  })
				.ToList();
			return Ok(result); // 返回结果
		}
	}
}
