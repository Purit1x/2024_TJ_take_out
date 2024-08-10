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
        
        public MerchantController(ApplicationDbContext context)
        {
            _context = context;
            _merchantService = new MerchantService(_context);
            
        }

        [HttpPost]
        [Route("register")]
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
        [Route("login")]
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
    }
}
