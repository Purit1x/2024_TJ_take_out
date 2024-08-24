﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.User;
using takeout_tj.Models.Merchant;
using takeout_tj.Service;

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
                merchant.TimeforOpenBusiness=dto.TimeforCloseBusiness;
                merchant.TimeforCloseBusiness=dto.TimeforOpenBusiness;
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

    }
}
