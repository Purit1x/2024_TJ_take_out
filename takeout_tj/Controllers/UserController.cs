using Microsoft.AspNetCore.Mvc;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.User;
using takeout_tj.Service;

namespace takeout_tj.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
            _userService = new UserService(_context);
        }

        // <summary>
        // 添加、多表添加、事务
        // </summary>
        // <returns></returns>
        [HttpPost]
        [Route("register")]
        public IActionResult AddUser([FromBody] UserDto dto)
        {
            var tran = _context.Database.BeginTransaction();//多表添加才用到
            try
            {
                UserDB user = new UserDB()
                {
                    UserId = _userService.AssignId(),
                    UserName = dto.UserName,
                    Password = dto.Password,
                    PhoneNumber = dto.PhoneNumber,
                };

                _context.Users.Add(user);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = user.UserId, msg = "注册成功" });
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

        /// <summary>
        /// 条件查询、模糊查询、查询全部、只查一条数据、排序、分组、分页、使用sql查询、多表查询-join(lanbda)、多表查询另一种写法(linq)、求总数、求和
        /// </summary>
        /// <param name="name">账号</param>
        /// <param name="pwd">密码</param>
        /// <returns></returns>
        [HttpPost]
        [Route("login")]
        public IActionResult Login([FromBody] UserDto user)
        {
            try
            {
                UserDB _user = new UserDB()
                {
                    UserId = user.UserId,
                    Password = user.Password
                };

                //var result = _context.Users.Where(n => n.UserName == user.UserName).ToList();

                // 条件查询
                var result1 = _context.Users.Where(n => n.UserId == _user.UserId && n.Password == _user.Password).ToList();

                // 模糊查询
                // var result2 = await _context.Users.Where(n => n.UserName.Contains("Test")).ToListAsync();

                // 查询全部
                var result3 = _context.Users.ToList();

                // 只查一条数据
                //var result4 = await _context.Users.FirstOrDefaultAsync();

                // 排序
                //var result5 = await _context.Users.OrderBy(n => n.AddTime).ToListAsync();

                // 分组
                //var result6 = _context.Users.AsEnumerable().GroupBy(n => n.UserName).ToList();

                // 分页
                // int pageIndex = 1;//分页索引：第几页
                // int pageSize = 10;//分页总数：一页多少条
                // var result7 = await _context.Users.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

                // 使用sql
                // sys_user为数据库表名，Users是实体名，不要混淆
                // String sql = string.Format($"select * from sys_user");
                // var res = _context.Users.FromSqlRaw(sql);
                // var result8 = await res.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

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
