using Microsoft.AspNetCore.Mvc;
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
                    Wallet = 0.00m // 初始化钱包  
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
                    merchant.MerchantName,
                    merchant.MerchantAddress,
                    merchant.Contact,
                    merchant.CouponType,
                    merchant.DishType,
                    merchant.TimeforOpenBusiness,
                    merchant.TimeforCloseBusiness,
                    merchant.Wallet // 添加钱包信息  
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
                merchant.TimeforOpenBusiness=dto.TimeforCloseBusiness;
                merchant.TimeforCloseBusiness=dto.TimeforOpenBusiness;

                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { msg = "用户信息更新成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 500, msg = "更新失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 500, msg = $"更新异常: {ex.Message}" });
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
    }
}
