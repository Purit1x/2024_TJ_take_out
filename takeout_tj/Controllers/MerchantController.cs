using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.User;
using takeout_tj.Models.Merchant;
using takeout_tj.Service;
using takeout_tj.Models.Platform;

namespace takeout_tj.Controllers
{
    [ApiController]  //表示这是 API 控制器
    [Route("[controller]")]  //定义路由模式，[controller] 替换为控制器名称，[action] 替换为方法名称
    public class MerchantController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly MerchantService _merchantService;
        private readonly string _uploadsFolder;

        public MerchantController(ApplicationDbContext context)
        {
            _context = context;
            _merchantService = new MerchantService(_context);
            _uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
            if (!Directory.Exists(_uploadsFolder))
            {
                Directory.CreateDirectory(_uploadsFolder); //创建文件夹  
            }

        }

        [HttpPost]
        [Route("register")]  //注册
        public IActionResult InitMerchant([FromBody] MerchantDBDto dto)
        {
            var tran = _context.Database.BeginTransaction();  //开启一个事务，确保出错可以回滚
            try
            {
                MerchantDB merchant = new MerchantDB()
                {
                    MerchantId = _merchantService.AssignId(),
                    Password = dto.Password,
                    MerchantName = dto.MerchantName,
                    MerchantAddress = dto.MerchantAddress,
                    Contact = dto.Contact,
                    CouponType = 0,
                    DishType = dto.DishType,
                    TimeforOpenBusiness = dto.TimeforOpenBusiness,
                    TimeforCloseBusiness = dto.TimeforCloseBusiness,
                    Wallet = 0.00m, // 初始化钱包  
                    WalletPassword=dto.WalletPassword,
                };
                _context.Merchants.Add(merchant);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data = merchant.MerchantId, msg = "注册成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();

                return StatusCode(30000, new { errorCode = 20000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpPost]
        [Route("login")] //登录
        public IActionResult Login([FromBody] MerchantDBDto merchant)
        {
            try
            {
                MerchantDB _merchant = new MerchantDB()
                {
                    MerchantId = merchant.MerchantId,
                    Password = merchant.Password
                };
                // 条件查询
                var result1 = _context.Merchants.Where(n => n.MerchantId == _merchant.MerchantId && n.Password == _merchant.Password).ToList();

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
        [Route("merchantSearch")]  //查询个人信息
        public IActionResult GetMerchantInfo(int merchantId)  //根据ID获取商家信息
        {
            try
            {
                // 查询指定 MerchantId 的商户信息  
                var merchant = _context.Merchants.FirstOrDefault(m => m.MerchantId == merchantId);

                if (merchant == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "商户未找到" });
                }

                // 获取商户的其他属性  
                var merchantInfo = new
                {
                    merchant.MerchantId,
                    merchant.Password,
                    merchant.MerchantName,
                    merchant.MerchantAddress,
                    merchant.Contact,
                    merchant.CouponType,
                    merchant.DishType,
                    merchant.TimeforOpenBusiness,
                    merchant.TimeforCloseBusiness,
                    merchant.Wallet,      // 添加钱包信息  
                    merchant.WalletPassword,
                };

                return Ok(new { data = merchantInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpGet]
        [Route("merchantAddrSearch")]  //查询商户的地址
        private string GetMerchantAddress(int merchantId)
        {
            try
            {
                // 查询指定 MerchantId 的商户信息  
                var merchant = _context.Merchants.FirstOrDefault(m => m.MerchantId == merchantId);

                if (merchant == null)
                {
                    return null;
                }

                return merchant.MerchantAddress;
            }
            catch (Exception ex)
            {
                throw new Exception($"无法获取商家地址: {ex.Message}", ex);
            }
        }

        [HttpPut]
        [Route("merchantEdit")]  //编辑个人信息
        public IActionResult EditMerchant([FromBody] MerchantDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                // 查找商家
                var merchant = _context.Merchants.FirstOrDefault(u => u.MerchantId == dto.MerchantId);
                if (merchant == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                // 更新商家信息  
                merchant.Password = dto.Password;
                merchant.MerchantName=dto.MerchantName;
                merchant.MerchantAddress=dto.MerchantAddress;
                merchant.Contact=dto.Contact;
                merchant.DishType=dto.DishType;
                merchant.CouponType = dto.CouponType;
                merchant.TimeforOpenBusiness=dto.TimeforOpenBusiness;
                merchant.TimeforCloseBusiness=dto.TimeforCloseBusiness;
                merchant.WalletPassword=dto.WalletPassword;

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
        public IActionResult WalletRecharge(int merchantId,int addMoney)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            { 
                var merchant = _context.Merchants.FirstOrDefault(u => u.MerchantId == merchantId);
                if (merchant == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                if(addMoney==0)
                {
                    tran.Commit();
                    return Ok(new { data = merchant.Wallet, msg = "用户信息更新成功" });
                }
                merchant.Wallet = merchant.Wallet + addMoney;
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data= merchant.Wallet,msg = "用户信息更新成功" });
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
        [HttpPut]
        [Route("withdraw")]
        public IActionResult WalletWithdraw(int merchantId, int withdrawMoney)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                var merchant = _context.Merchants.FirstOrDefault(u => u.MerchantId == merchantId);
                if (merchant == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                if (withdrawMoney == 0)
                {
                    tran.Commit();
                    return Ok(new { data = merchant.Wallet, msg = "用户信息更新成功" });
                }
                merchant.Wallet = merchant.Wallet - withdrawMoney;
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data = merchant.Wallet, msg = "用户信息更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "提现失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 30000, msg = $"提现异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("dishSearch")]
        public IActionResult GetDishesByMerchantId(int merchantId)
        {
            try
            {
                // 查询符合指定 MerchantId 的所有菜品  
                var dishes = _context.Dishes.Where(d => d.MerchantId == merchantId).ToList();

                if (!dishes.Any())
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "未找到任何菜品" });
                }
                // 获取菜品的相关信息  
                var dishList = dishes.Select(d => new
                {
                    d.MerchantId,
                    d.DishId,
                    d.DishName,
                    d.DishPrice,
                    d.DishCategory,
                    d.ImageUrl,
                    d.DishInventory
                }).ToList();

                return Ok(new { data = dishList, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpDelete]
        [Route("dishDelete")]
        public IActionResult DeleteDish([FromQuery] int merchantId, [FromQuery] int dishId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的菜品  
                var dish = _context.Dishes.FirstOrDefault(d => d.DishId == dishId && d.MerchantId == merchantId);
                if (dish == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "菜品未找到" });
                }
                // 删除菜品前先删除对应的图片文件  
                if (!string.IsNullOrEmpty(dish.ImageUrl)) // 确保图片 URL 不为空  
                {
                    var fileName = Path.GetFileName(dish.ImageUrl); // 从 URL 中提取文件名  
                    var filePath = Path.Combine(_uploadsFolder, fileName);

                    if (System.IO.File.Exists(filePath)) // 检查文件是否存在  
                    {
                        System.IO.File.Delete(filePath); // 删除文件  
                    }
                }
                // 删除菜品  
                _context.Dishes.Remove(dish);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "菜品删除成功" });
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
        [HttpPost("uploadImage")]
        public async Task<IActionResult> UploadImage(IFormFile file)  //上传图片转URL
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var fileName = $"{Path.GetFileNameWithoutExtension(file.FileName)}_{DateTime.Now.Ticks}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream); //保存文件  
            }

            var imageUrl = $"http://localhost:5079/uploads/{fileName}"; //生成URL  
            return Ok(new { url = imageUrl }); //返回JSON  
        }
        [HttpPut("dishEdit")]
        public IActionResult EditDish([FromBody] DishDBDto dishDto)
        {
            var tran = _context.Database.BeginTransaction(); // 开启一个事务  
            try
            {
                // 查找要更新的菜品  
                var dish = _context.Dishes.FirstOrDefault(d => d.DishId == dishDto.DishId && d.MerchantId == dishDto.MerchantId);
                if (dish == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "菜品未找到" });
                }

                // 更新菜品属性  
                dish.DishName = dishDto.DishName;
                dish.DishPrice = dishDto.DishPrice;
                dish.DishCategory = dishDto.DishCategory;
                dish.ImageUrl = dishDto.ImageUrl;
                dish.DishInventory = dishDto.DishInventory;

                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data=dish,msg = "菜品更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "更新失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"更新异常: {ex.Message}" });
            }
        }
        [HttpPost("createDish")]
        public IActionResult CreateDish([FromBody] DishDBDto dishDto)
        {
            var tran = _context.Database.BeginTransaction(); // 开启一个事务  
            try
            {
                // 校验输入  
                if (dishDto == null)
                {
                    return BadRequest(new { errorCode = 40000, msg = "请求数据无效" });
                }
                // 创建新的菜品实体  
                DishDB newDish = new DishDB()
                {
                    DishId = _merchantService.AssignDishId(), //按照顺序分配Id 
                    MerchantId = dishDto.MerchantId,
                    DishName = dishDto.DishName,
                    DishPrice = dishDto.DishPrice,
                    DishCategory = dishDto.DishCategory,
                    ImageUrl = dishDto.ImageUrl,
                    DishInventory = dishDto.DishInventory
                };

                // 保存新增的菜品到数据库  
                _context.Dishes.Add(newDish);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data = newDish.DishId, msg = "菜品创建成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpGet("GetStations")]
        public async Task<IActionResult> GetAllStationInfos()
        {
            var stations = await _context.Stations
                .Select(m => new
                {
                    m.StationId,         
                    m.StationName,       
                    m.StationAddress     
                                     
                })
                .ToListAsync(); // 异步获取列表  
            return Ok(new { data = stations }); // 返回站点
        }
        [HttpPost]
        [Route("assignStation")]  //为商家分配站点
        public IActionResult AssignStation([FromBody] MerchantStationDBDto dto)
        {
            var tran = _context.Database.BeginTransaction();  //开启一个事务，确保出错可以回滚
            try
            {
                var mS = _context.MerchantStations.FirstOrDefault(d => d.MerchantId == dto.MerchantId);
                if (mS != null)  //已经有该元素，无需创建
                {
                    return Ok(new { data = mS, msg = "站点分配已创建" });
                }
                MerchantStationDB merchantStation = new MerchantStationDB()
                {
                    MerchantId = dto.MerchantId,
                    StationId = dto.StationId,
                };
                _context.MerchantStations.Add(merchantStation);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data = merchantStation.StationId, msg = "分配成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "分配失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();

                return StatusCode(30000, new { errorCode = 20000, msg = $"分配异常: {ex.Message}" });
            }
        }
        [HttpPut]
        [Route("editMerchantStation")]
        public IActionResult EditMerchantStation([FromBody] MerchantStationDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开启一个事务  
            try
            {
                // 查找要更新的站点分配
                var merchantStation = _context.MerchantStations.FirstOrDefault(d => d.MerchantId==dto.MerchantId);
                if (merchantStation == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "站点分配未找到" });
                }
                if (merchantStation.StationId == dto.StationId)
                {
                    tran.Commit();
                    return Ok(new { data = merchantStation, msg = "站点分配未更新" });
                }
                // 更新站点分配  
                merchantStation.StationId = dto.StationId;

                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data = merchantStation, msg = "站点分配更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "更新失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"更新异常: {ex.Message}" });
            }
        }

        [HttpPost]
        [Route("specialOfferCreate")]
        public IActionResult AddSpecialOffer([FromBody] SpecialOfferDBDto dto)  //创建满减服务
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                if(dto.MinPrice < dto.AmountRemission)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败，满减金额不可大于满减门槛！" });
                }

                SpecialOfferDB specialOffer = new SpecialOfferDB()
                {
                    MerchantId = dto.MerchantId,
                    OfferId = _merchantService.AssignSpecialOfferId(),
                    MinPrice = dto.MinPrice,
                    AmountRemission = dto.AmountRemission
                };

                _context.SpecialOffers.Add(specialOffer);

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = specialOffer, msg = "创建成功" });
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

        [HttpPut]
        [Route("specialOfferEdit")]  //编辑满减服务
        public IActionResult EditSpecialOffer([FromBody] SpecialOfferDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                var offer = _context.SpecialOffers.FirstOrDefault(u => u.OfferId == dto.OfferId);
                if (offer == null)
                {
                    return NotFound(new { errorCode = 404, msg = "特殊服务未找到" });
                }

                if (dto.MinPrice < dto.AmountRemission)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "修改失败，满减金额不可大于满减门槛！" });
                }

                offer.MerchantId = dto.MerchantId;
                offer.MinPrice = dto.MinPrice;
                offer.AmountRemission = dto.AmountRemission;

                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data = offer, msg = "服务信息更新成功" });
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
        [Route("deleteSpecialOffer")]
        public IActionResult DeleteOffer([FromQuery] int offerId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的服务
                var offer = _context.SpecialOffers.FirstOrDefault(d => d.OfferId == offerId);
                if (offer == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "服务未找到" });
                }
      
                // 删除服务 
                _context.SpecialOffers.Remove(offer);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "删除成功" });
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

        [HttpGet]
        [Route("specialOfferGet")]
        public IActionResult GetOffersInfo(int merchantId)  //查询某商户的特殊服务
        {
            try
            {
                var offers = _context.SpecialOffers.Where(m => m.MerchantId == merchantId).ToList();

                if (offers.Count == 0)
                {
                    return Ok(new { data = offers, msg = "无满减活动" });
                }

                return Ok(new { data = offers, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpGet]
        [Route("getDishInfo")]
        public IActionResult getDishInfo(int merchantId, int dishId)
        {
            try
            {
                var dish = _context.Dishes.FirstOrDefault(ou => ou.DishId == dishId && ou.MerchantId == merchantId);
                if (dish != null)
                {
                    return Ok(new { data = dish, msg = "查找成功" }); // 返回找到的订单  
                }
                else
                {
                    return NotFound(new { msg = "未找到相关菜品" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("multiSpecialOfferGet")]
        public IActionResult GetMultiOffersInfo([FromQuery] List<int> merchantIds)  // 查询多个商户的特殊服务
        {
            try
            {
                if (merchantIds == null || !merchantIds.Any())
                {
                    return BadRequest(new { errorCode = 40000, msg = "商户ID列表为空" });
                }

                var offers = _context.SpecialOffers
                    .Where(m => merchantIds.Contains(m.MerchantId))
                    .ToList();

                if (offers.Count == 0)
                {
                    return Ok(new { data = offers, msg = "无满减活动" });
                }

                return Ok(new { data = offers, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpGet]
        [Route("getOrdersToHandle")]
        public IActionResult getOrdersToHandle(int merchantId)
        {
            try
            {
                var orders = _context.OrderDishes
                    .Include(ou => ou.OrderDB) // 包含订单信息  
                    .Where(ou => ou.MerchantId == merchantId)
                    .Select(ou => ou.OrderDB)
                    .Where(ou=>ou.State==1||ou.State==2||ou.State==3)
                    .Distinct() // 确保唯一性
                    .ToList();
                if (orders.Count > 0)
                {
                    return Ok(new { data = orders, msg = "查找成功" }); // 返回找到的订单  
                }
                else
                {
                    return Ok(new { data = 40000, msg = "未找到相关订单" });
                }


            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
        [HttpDelete]
        [Route("deletePaidOrder")]
        public IActionResult deletePaidOrder(int orderId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                var order=_context.Orders.FirstOrDefault(o => o.OrderId == orderId);
                if (order == null)
                {
                    return StatusCode(40000,new { errorCode = 40000, msg = "未找到要删除的订单" });
                }
                var orderDishes = _context.OrderDishes.Where(od => od.OrderId == order.OrderId).ToList();
                foreach (var orderDish in orderDishes)
                {
                    // 根据 DishId 查询菜品  
                    var dish =  _context.Dishes.FirstOrDefault(d => d.DishId == orderDish.DishId);
                    if (dish != null)
                    {
                        // 恢复库存  
                        dish.DishInventory += orderDish.DishNum; // 增加菜品库存  
                        _context.Dishes.Update(dish); // 确保将更新的菜品存回上下文  
                    }
                }
    
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
                    var user = _context.Users.FirstOrDefault(u => u.UserId == orderUser.UserId);
                    if (user != null)
                    {
                        user.Wallet += order.Price;
                    }
                }
                // 删除订单
                _context.Orders.Remove(order);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "订单删除成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"查询异常: {ex.Message}" });
            }
        }
		[HttpGet]
		[Route("getMerAddrByOrderId")]
		public async Task<IActionResult> GetMerAddrByOrderId(int orderId)
		{
			try
			{
				var orderDish = await _context.OrderDishes.FirstOrDefaultAsync(od => od.OrderId == orderId);
				if (orderDish == null)
				{
					return NotFound(new { errorCode = 404, msg = "指定订单不存在" });
				}
				var merchantId = orderDish.MerchantId;
				var merchant = await _context.Merchants
					.FirstOrDefaultAsync(m => m.MerchantId == merchantId);
				if (merchant == null)
				{
					return NotFound(new { errorCode = 404, msg = "商户未找到" });
				}
				return Ok(new { data = merchant.MerchantAddress, msg = "获取成功" });
			}
			catch (Exception ex)
			{
				return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, msg = ex.Message });
			}
		}

		[HttpPut]
		[Route("deliverOrder")]
		public async Task<IActionResult> DeliverOrder([FromBody] DeliverOrderDto deliverDto)
		{
			using (var transaction = _context.Database.BeginTransaction())
			{
				try
				{
					// 查找订单和骑手信息
					var orderRider = await _context.OrderRiders
						.Include(or => or.OrderDB)
						.Include(or => or.RiderDB)
						.FirstOrDefaultAsync(or => or.OrderId == deliverDto.OrderId);

					if (orderRider == null)
					{
						transaction.Rollback(); // 回滚事务
						return NotFound(new { errorCode = 404, msg = "订单未找到" });
					}

					// 确认订单状态是骑手派送中
					if (orderRider.OrderDB.State != 2)
					{
						transaction.Rollback(); // 回滚事务
						return BadRequest(new { errorCode = 400, msg = "订单状态无效，无法标记" });
					}

					// 更新订单状态
					orderRider.OrderDB.State = 3; // 已送达
					orderRider.OrderDB.RealTimeOfArrival = DateTime.Now;

					// 更新骑手钱包余额
					orderRider.RiderDB.Wallet += orderRider.RiderPrice;
                    //默认评价为5；
                    orderRider.OrderDB.MerchantRating = 5;
                    orderRider.OrderDB.RiderRating = 5;
					// 获取订单中的所有菜品信息
					var orderDishes = await _context.OrderDishes
						.Where(od => od.OrderId == deliverDto.OrderId)
						.ToListAsync();

					if (!orderDishes.Any())
					{
						transaction.Rollback(); // 回滚事务
						return BadRequest(new { errorCode = 400, msg = "订单中没有菜品信息" });
					}

					// 获取第一个菜品的MerchantId，假设所有菜品来自同一个商家
					var merchantId = orderDishes.FirstOrDefault()?.MerchantId;

					if (merchantId == null)
					{
						transaction.Rollback(); // 回滚事务
						return BadRequest(new { errorCode = 400, msg = "无法获取商家ID" });
					}

					// 获取商家信息
					var merchant = await _context.Merchants.FirstOrDefaultAsync(m => m.MerchantId == merchantId);

					if (merchant == null)
					{
						transaction.Rollback(); // 回滚事务
						return BadRequest(new { errorCode = 400, msg = "商家不存在" });
					}

					// 更新商家钱包余额
					merchant.Wallet += orderRider.OrderDB.Price - orderRider.RiderPrice;
                    
					// 提交更改
					await _context.SaveChangesAsync();

					// 提交事务
					transaction.Commit();

					return Ok(new { msg = "订单已成功标记为已送达" });
				}
				catch (DbUpdateException ex)
				{
					transaction.Rollback(); // 回滚事务
					return StatusCode(500, new { errorCode = 500, msg = "数据库更新失败" });
				}
				catch (Exception ex)
				{
					transaction.Rollback(); // 回滚事务
					return StatusCode(500, new { errorCode = 500, msg = "服务器内部错误" });
				}
			}
		}
		
		
		[HttpGet("ordersByRegion")]
		public IActionResult GetOrdersByRegion()
		{
			var ordersByRegion = _context.Set<OrderDB>()
				.GroupBy(o => o.AddressId)
				.Select(g => new { Region = g.Key, Count = g.Count() })
				.ToList();

			return Ok(ordersByRegion);
		}
        [HttpGet]
        [Route("getMerOrdersWithinThisMonth")]
        public async Task<IActionResult> GetMerOrdersWithinThisMonth(int merchantId)
        {
            try
            {
                var orderMerchant = await _context.OrderDishes
                    .Include(ou => ou.OrderDB)
                    .Where(ou => ou.MerchantId == merchantId)
                    .Select(ou => ou.OrderId)
                    .ToListAsync();//获取指定商家的所有订单；
                if(!orderMerchant.Any())
                {
                    return Ok(new { data = 0, msg = "指定商家无订单" });
                }
                var currentDate = DateTime.Now;
                var orders = await _context.Orders
                    .Where(o => o.OrderTimestamp.Year == currentDate.Year && o.OrderTimestamp.Month == currentDate.Month
                    && orderMerchant.Contains(o.OrderId) && o.State == 3)
                    .Select(o => o.OrderId)
                    .ToListAsync();
                if(!orderMerchant.Any())
                {
                    return Ok(new { data = 0, msg = "指定商家本月内无订单" });
                }
                return Ok(new { data = orders, msg = "获取成功" });
			}
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
		[HttpGet]
		[Route("getMerOrdersWithinThisDay")]
		public async Task<IActionResult> GetMerOrdersWithinThisDay(int merchantId)
		{
			try
			{
				var orderMerchant = await _context.OrderDishes
					.Include(ou => ou.OrderDB)
					.Where(ou => ou.MerchantId == merchantId&&ou.OrderDB.State==3)
					.Select(ou => ou.OrderId)
					.ToListAsync();//获取指定商家的所有订单；
				if (!orderMerchant.Any())
				{
					return Ok(new { data = 0, msg = "指定商家无订单" });
				}
				var currentDate = DateTime.Now;
				var orders = await _context.Orders
					.Where(o => o.OrderTimestamp.Year == currentDate.Year && o.OrderTimestamp.Month == currentDate.Month&& o.OrderTimestamp.Day == currentDate.Day
					&& orderMerchant.Contains(o.OrderId) && o.State == 3)
					.Select(o => o.OrderId)
					.ToListAsync();
				if (!orderMerchant.Any())
				{
					return Ok(new { data = 0, msg = "指定商家本日内无订单" });
				}
				return Ok(new { data = orders, msg = "获取成功" });
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
        [HttpGet]
        [Route("getMerPrice")]
        public async Task<IActionResult>  GetMerPrice(int orderId)
        {
            try
            {
                var orderMer = await _context.Orders.FirstOrDefaultAsync(or => or.OrderId == orderId);
                var orderRider = await _context.OrderRiders.FirstOrDefaultAsync(or => or.OrderId == orderId);
				if (orderMer == null || orderRider == null)
				{
					return NotFound(new { errorCode = 404, msg = "指定订单不存在" });
				}
				return Ok(new { data = orderMer.Price - orderRider.RiderPrice, msg = "获取成功" });
			}
            catch(Exception ex)
            {
				return StatusCode(StatusCodes.Status500InternalServerError, new { errorCode = 500, msg = "获取失败" });
			}
        }
        [HttpGet]
        [Route("getMerAvgRating")]
		public async Task<IActionResult> GetMerAvgRating(int merchantId)
        {
            try
            {
				var orderMerchant = await _context.OrderDishes
					.Include(ou => ou.OrderDB)
					.Where(ou => ou.MerchantId == merchantId)
					.Select(ou => ou.OrderId)
					.ToListAsync();//获取指定商家的所有订单；
				if (!orderMerchant.Any())
				{
					return Ok(new { data = 0, msg = "指定商家无订单" });
				}

                var orderNum= orderMerchant.Count();
				var ratings = await _context.OrderDishes
					.Include(ou => ou.OrderDB)
					.Where(ou => ou.MerchantId == merchantId)
					.Select(ou => ou.OrderDB.MerchantRating)
					.ToListAsync();//获取指定商家的所有评分；
                if(!ratings.Any())
                {
                    return Ok(new { data = 0, msg = "该商家暂无评分记录" });
                }
                double sum = 0;
                foreach (var rating in ratings)
                {
                    sum += rating??0;
                }
				double avgRating = orderNum > 0 ? Math.Round(sum / orderNum, 2, MidpointRounding.AwayFromZero) : 0;
				return Ok(new { data = avgRating, msg = "成功" });


			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
        }
        [HttpGet]
        [Route("getFinishedMerOrders")]
        public async Task<IActionResult>GetFinishedMerOrders(int merchantId)
        {
            try
            {
				var orderMerchant = await _context.OrderDishes
					.Include(ou => ou.OrderDB)
					.Where(ou => ou.MerchantId == merchantId)
					.Select(ou => ou.OrderId)
					.ToListAsync();//获取指定商家的所有订单；
				if (!orderMerchant.Any())
				{
					return Ok(new { data = 0, msg = "指定商家无订单" });
				}
                var orders = await _context.Orders
                    .Where(o => orderMerchant.Contains(o.OrderId) && o.State == 3)
                    .ToListAsync();
                if(!orders.Any())
                {
                    return Ok(new { data = 0, msg = "该商家尚无已送达订单" });
                }
                return Ok(new { data = orders, msg = "获取成功" });
			}
            catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpGet]
        [Route("getMerRating")]
        public async Task<IActionResult>GetMerRating(int orderId)
        {
            try
            {
                var orders = await _context.Orders
                    .Where(ou => ou.OrderId == orderId)
                    .Select(ou => ou.MerchantRating)
                    .ToListAsync();

				if (!orders.Any())
				{
					return Ok(new { data = 0, msg = "无评分" });
				}
                return Ok(new { data = orders, msg = "获取成功" });
			}
			catch (Exception ex)
			{
				return BadRequest(ex.Message);
			}
		}
		/*[HttpGet]
        [Route("getSortedMerchaants")]
        public IActionResult GetSortedMerchants()
        {
			// 获取所有商家的信息，并按评分降序，销售额降序排序
			var sortedMerchants = _context.Set<MerchantDB>()
				.OrderByDescending(m => m.MerchantRating)  // 按评分降序
				.ThenByDescending(m => m.TotalSalesAmount) // 按销售额降序
				.ToList();

			return Ok(sortedMerchants);
		}*/
	}
}
