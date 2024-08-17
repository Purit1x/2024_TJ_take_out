using Microsoft.AspNetCore.Mvc;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Platform;
using takeout_tj.Service;

namespace takeout_tj.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PlatformController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PlatformService _riderService;
        public PlatformController(ApplicationDbContext context)
        {
            _context = context;
            _riderService = new PlatformService(_context);
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
    }
}
