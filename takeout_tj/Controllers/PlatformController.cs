using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Merchant;
using takeout_tj.Models.Platform;
using takeout_tj.Models.Rider;
using takeout_tj.Service;
using takeout_tj.Controllers;

namespace takeout_tj.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PlatformController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PlatformService _platformService;
        public PlatformController(ApplicationDbContext context)
        {
            _context = context;
            _platformService = new PlatformService(_context);
        }
        [HttpPost]
        [Route("login")] //登录
        public IActionResult Login([FromBody] AdminDBDto admin)
        {
            try
            {
                AdminDB _admin = new AdminDB()
                {
                    AdminId = admin.AdminId,
                    Password = admin.Password
                };
                // 条件查询
                var result1 = _context.Admins.Where(n => n.AdminId == _admin.AdminId && n.Password == _admin.Password).ToList();

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
        [HttpDelete]
        [Route("merchantDelete")]
        public IActionResult DeleteMerchant([FromQuery] int merchantId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的菜品  
                var merchant = _context.Merchants.FirstOrDefault(d => d.MerchantId == merchantId);
                if (merchant == null)
                {
                    return StatusCode(404, new { errorCode = 404, msg = "商家未找到" });
                }
                // 删除菜品  
                _context.Merchants.Remove(merchant);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "商家删除成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"删除异常: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("stationCreate")]
        public IActionResult AddStation([FromBody] StationDBDto dto)  //创建站点
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                StationDB station = new StationDB()
                {
                    StationId = _platformService.AssignStationId(),
                    StationName = dto.StationName,
                    StationAddress = dto.StationAddress,
                };
                _context.Stations.Add(station);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = station.StationId, msg = "创建成功" });
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
        [HttpGet]
        [Route("stationSearch")]
        public IActionResult GetStationInfo(int stationId)  //查询站点
        {
            try
            {
                var station = _context.Stations.FirstOrDefault(m => m.StationId == stationId);

                if (station == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "站点未找到" });
                }

                // 获取商户的其他属性  
                var stationInfo = new
                {
                    station.StationId,
                    station.StationName,
                    station.StationAddress,
                };

                return Ok(new { data = stationInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPut]
        [Route("stationEdit")]  //编辑站点
        public IActionResult EditStation([FromBody] StationDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                // 查找商家
                var station = _context.Stations.FirstOrDefault(u => u.StationId == dto.StationId);
                if (station == null)
                {
                    return NotFound(new { errorCode = 404, msg = "站点未找到" });
                }
                station.StationName = dto.StationName;
                station.StationAddress = dto.StationAddress;

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { msg = "站点信息更新成功" });
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
        [HttpDelete]
        [Route("stationDelete")]
        public IActionResult DeleteStation([FromQuery] int stationId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的菜品  
                var station = _context.Stations.FirstOrDefault(d => d.StationId == stationId);
                if (station == null)
                {
                    return StatusCode(404, new { errorCode = 404, msg = "未找到" });
                }
                // 删除菜品  
                _context.Stations.Remove(station);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "站点删除成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"删除异常: {ex.Message}" });
            }
        }
        [HttpGet("GetStationIds")]
        public async Task<IActionResult> GetAllStationIds()
        {
            var stationIds = await _context.Stations
                .Select(m => m.StationId)
                .ToListAsync(); // 异步获取列表  

            return Ok(new { data = stationIds }); // 返回商家 ID 列表  
        }
        [HttpGet("SeparateRiders")]
        public IActionResult GetUnassignedRiders()  //分开已分配站点和未分配站点的骑手
        {
            try
            {
                // 查询所有 Riders 的 ID  
                var allRiders = _context.Riders.ToList(); // 假设 YourDbContext 中有 Riders 集合   

                // 查询所有已分配站点的 Rider IDs  
                var assignedRiderIds = _context.RiderStations.Select(rs => rs.RiderId).Distinct().ToList();

                // 找出未分配的 Rider IDs  
                var unassignedRiderIds = allRiders
                    .Where(r => !assignedRiderIds.Contains(r.RiderId))
                    .Select(r => r.RiderId)
                    .ToList();
                var assignedRiders = allRiders
                    .Where(r => assignedRiderIds.Contains(r.RiderId))
                    .Select(r => r.RiderId)
                    .ToList();
                return Ok(new { data1 = unassignedRiderIds, data2 = assignedRiders });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 500, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("riderStationAssign")]
        public IActionResult AddRiderStation([FromBody] RiderStationDBDto dto)  //为骑手分配站点
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                RiderStationDB riderStation = new RiderStationDB()
                {
                    StationId = dto.StationId,
                    RiderId = dto.RiderId,
                };
                _context.RiderStations.Add(riderStation);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { msg = "分配成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "分配失败" });
                }

            }
            catch (Exception ex)
            {
                tran.Rollback();    //多表添加才用到

                return StatusCode(20000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpDelete]
        [Route("riderStationDelete")]
        public IActionResult DeleteRiderStation([FromQuery] int riderId)  //删除站点分配
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                var riderStation = _context.RiderStations.FirstOrDefault(d => d.RiderId == riderId);
                if (riderStation == null)
                {
                    return StatusCode(404, new { errorCode = 404, msg = "未找到" });
                }
                _context.RiderStations.Remove(riderStation);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "站点分配删除成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"删除异常: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("riderStationEdit")]  //编辑站点分配
        public IActionResult EditRiderStation([FromBody] RiderStationDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                // 查找商家
                var riderStation = _context.RiderStations.FirstOrDefault(u => u.RiderId == dto.RiderId);
                if (riderStation == null)
                {
                    return NotFound(new { errorCode = 404, msg = "站点分配未找到" });
                }
                riderStation.StationId = dto.StationId;

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { msg = "站点分配更新成功" });
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
        [HttpPost]
        [Route("couponCreate")]
        public IActionResult AddCoupon([FromBody] CouponDBDto dto)  //创建优惠券
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                CouponDB coupon = new CouponDB()
                {
                    CouponId = _platformService.AssignCouponId(),
                    CouponName = dto.CouponName,
                    CouponValue = dto.CouponValue,
                    CouponPrice = dto.CouponPrice,
                    CouponType = dto.CouponType,
                    MinPrice = dto.MinPrice,
                    PeriodOfValidity = dto.PeriodOfValidity,
                    QuantitySold = 0,
                    IsOnShelves = 1,
                };
                _context.Coupons.Add(coupon);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = coupon.CouponId, msg = "创建成功" });
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
        [HttpGet]
        [Route("couponSearch")]
        public IActionResult GetCouponInfo(int couponId)  //查询优惠券
        {
            try
            {
                var coupon = _context.Coupons.FirstOrDefault(m => m.CouponId == couponId);

                if (coupon == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "优惠券未找到" });
                }

                // 获取商户的其他属性  
                var couponInfo = new
                {
                    coupon.CouponId,
                    coupon.CouponName,
                    coupon.CouponValue,
                    coupon.CouponPrice,
                    coupon.CouponType,
                    coupon.MinPrice,
                    coupon.PeriodOfValidity,
                    coupon.QuantitySold,
                    coupon.IsOnShelves
                };

                return Ok(new { data = couponInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPut]
        [Route("couponEdit")]  //编辑优惠券
        public IActionResult EditCoupon(int couponId, int isOnShelves)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                var coupon = _context.Coupons.FirstOrDefault(u => u.CouponId == couponId);
                if (coupon == null)
                {
                    return NotFound(new { errorCode = 404, msg = "优惠券未找到" });
                }
                coupon.IsOnShelves = isOnShelves;

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { msg = "优惠券信息更新成功" });
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
        [HttpGet("GetCouponIds")]
        public async Task<IActionResult> GetAllCouponIds()
        {
            var couponIds = await _context.Coupons
                .Select(m => m.CouponId)
                .ToListAsync(); // 异步获取列表  
            return Ok(new { data = couponIds }); // 返回商家 ID 列表  
        }
        [HttpGet]
        [Route("GetMerchantStation")]  // 获取商家对应站点
        public async Task<IActionResult> GetMerchantStation(int merchantId)
        {
            var stationId = await _context.MerchantStations
                .FirstOrDefaultAsync(ms => ms.MerchantId == merchantId);

            if (stationId == null)
                return NotFound(new { errorCode = 404, msg = "指定ID商家不存在或无对应站点" });

            return Ok(new { data = stationId.StationId });
        }
        [HttpGet]
        [Route("GetStationRiders")]  // 获取指定站点所有骑手
        public async Task<IActionResult> GetStationRiders(int stationId)
        {
            var riders = await _context.RiderStations
                .Where(rs => rs.StationId == stationId)
                .Select(rs => rs.RiderId)
                .ToListAsync();

            if (!riders.Any())
                return NotFound(new { errorCode = 404, msg = "指定站点无骑手" });

            return Ok(new { data = riders });
        }
        [HttpGet]
        [Route("getEcoOrder")]//查找不需要餐具的订单比例
        public IActionResult GetEcoOrder()
        {
            int totalOrders = _context.Set<OrderDB>().Count();

            int ecoFriendlyOrders = _context.Set<OrderDB>()
           .Count(o => o.NeedUtensils == 0);  // 0 表示无需餐具

            double ecoOrderRatio = totalOrders > 0 ? (double)ecoFriendlyOrders / totalOrders * 100 : 0;
            return Ok(new { EcoOrderRatio = $"{ecoOrderRatio:F2}%" }); // 返回结果
        }
        [HttpGet]
        [Route("getMerAddrByOrderId")]
        public async Task<string> GetMerAddrByOrderId(int orderId)
        {
            try
            {
                var orderDish = await _context.OrderDishes.FirstOrDefaultAsync(od => od.OrderId == orderId);
                if (orderDish == null)
                {
                    return null;
                }
                var merchantId = orderDish.MerchantId;
                var merchant = await _context.Merchants
                    .FirstOrDefaultAsync(m => m.MerchantId == merchantId);
                if (merchant == null)
                {
                    return null;
                }
                return merchant.MerchantAddress;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private bool MerchantAddressContainsRegion(string merchantAddress, string region)
        {
            if (merchantAddress != null)
            {
                return merchantAddress.Contains(region);
                // 假设merchantAddress是一个以逗号分隔的地区列表，我们可以用此方式来检查是否包含特定的地区
                return merchantAddress.Split(',').Any(addrPart => addrPart.Trim().Equals(region, StringComparison.OrdinalIgnoreCase));
            }
            return false;
        }
        [HttpGet]
        [Route("GetSalesByRegion")]
        public async Task<IActionResult> GetSalesByRegion(string region)
        {
            try
            {
                // 使用LINQ查询指定地区（部分匹配）的销售额
                var orders = await _context.Orders.ToListAsync();
                var filteredOrders = orders
                    .Where(order =>
                    {
                        string merchantAddress = GetMerAddrByOrderId(order.OrderId).Result;
                        return MerchantAddressContainsRegion(merchantAddress, region);
                    });
                var totalSales = filteredOrders.Sum(order => order.Price);


                if (totalSales >= 0)
                {
                    return Ok(new { Region = region, TotalSales = totalSales });
                }
                else
                {
                    return NotFound(new { errorCode = 404, msg = "没有找到包含指定地区的销售记录" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 500, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("GetQuantityByRegion")]
        public async Task<IActionResult> GetQuantityByRegion(string region)
        {
            try
            {
                // 使用LINQ查询指定地区（部分匹配）的订单量
                var orders = await _context.Orders.ToListAsync();
                var filteredOrders = orders
                    .Where(order =>
                    {
                        string merchantAddress = GetMerAddrByOrderId(order.OrderId).Result;
                        return MerchantAddressContainsRegion(merchantAddress, region);
                    })
                    .ToList(); // 确保在这里调用 .ToList() 以执行查询

                var orderCount = filteredOrders.Count; // 获取满足条件的订单数量

                if (orderCount > 0)
                {
                    return Ok(new { Region = region, TotalSales = orderCount });
                }
                else
                {
                    return NotFound(new { errorCode = 404, msg = "没有找到包含指定地区的订单量" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { errorCode = 500, msg = $"查询异常: {ex.Message}" });
            }
        }


    }
		
}


