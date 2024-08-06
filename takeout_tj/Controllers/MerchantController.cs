using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.User;
using takeout_tj.Models.Merchant;

namespace takeout_tj.Controllers
{
    [ApiController]  //表示这是 API 控制器
    [Route("[controller]")]  //定义路由模式，[controller] 替换为控制器名称，[action] 替换为方法名称
    public class MerchantController : Controller
    {
        private readonly ApplicationDbContext _context;
        public MerchantController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("Merchant")]
        public IActionResult InitMerchant([FromBody] MerchantDBDto dto)
        {
            var tran = _context.Database.BeginTransaction();  //开启一个事务，确保出错可以回滚
            try
            {
                MerchantDB merchant = new MerchantDB()
                {
                    MerchantId = dto.MerchantId,  //取决于登录信息
                    Password=dto.Password,
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
                    return StatusCode(200, new { data = merchant.MerchantId, msg = "ok" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();

                return StatusCode(20000, new { errorCode = 20000, msg = $"创建异常: {ex.Message}" });
            }
        }

    }
}
