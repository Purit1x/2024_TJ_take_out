using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Rider;
using takeout_tj.Service;

namespace takeout_tj.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class RiderController:Controller
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
                return Ok(new { data=riderStation.StationId, msg = "获取成功" });
            }
            catch(Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
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
                    var orderDishes=_context.OrderDishes.Where(od=>od.OrderId== order.OrderId).ToList();
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
                    return NotFound(new { errorCode = 40000, msg = "未找到相关订单" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("receiveOrder")]
        public IActionResult receiveOrder(int riderId,int orderId)
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
                    return NotFound(new { errorCode = 40000, msg = "未找到相关订单" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
    }
}
