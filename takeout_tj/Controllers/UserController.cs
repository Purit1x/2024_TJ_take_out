using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Merchant;
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
                    Wallet = 0.00m, // 初始化钱包
                    WalletPassword = dto.WalletPassword,
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
        [HttpGet]
        [Route("userSearch")]  //查询个人信息
        public IActionResult GetMerchantInfo(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(m => m.UserId == userId);

                if (user == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "用户未找到" });
                }

                var userInfo = new
                {
                    user.UserId,
                    user.UserName,
                    user.PhoneNumber,
                    user.Password,
                    user.Wallet,
                    user.WalletPassword,
                };

                return Ok(new { data = userInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPut]
        [Route("userEdit")]  //编辑个人信息
        public IActionResult EditMerchant([FromBody] UserDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务  
            try
            {
                // 查找商家
                var user = _context.Users.FirstOrDefault(u => u.UserId == dto.UserId);
                if (user == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                // 更新商家信息  
                user.UserName = dto.UserName;
                user.PhoneNumber = dto.PhoneNumber;
                user.Password = dto.Password;
                user.Wallet = dto.Wallet;
                user.WalletPassword = dto.WalletPassword;

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
        public IActionResult WalletRecharge(int userId, int addMoney)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                if (addMoney == 0)
                {
                    tran.Commit();
                    return Ok(new { data = user.Wallet, msg = "用户信息更新成功" });
                }
                user.Wallet = user.Wallet + addMoney;
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data = user.Wallet, msg = "用户信息更新成功" });
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
        // 获取所有商家的 MerchantId  
        [HttpGet("GetMerchantIds")]
        public async Task<IActionResult> GetAllMerchantIds()
        {
            // 从数据库中获取所有商家的 MerchantId  
            var merchantIds = await _context.Merchants
                .Select(m => m.MerchantId) // 选择 MerchantId  
                .ToListAsync(); // 异步获取列表  

            return Ok(new { data = merchantIds }); // 返回商家 ID 列表  
        }
        [HttpGet]
        [Route("merchantsSearch")]  //查询个人信息
        public IActionResult GetMerchantsInfo(int merchantId)  //根据ID获取商家信息
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
                    //merchant.Password,
                    merchant.MerchantName,
                    merchant.MerchantAddress,
                    merchant.Contact,
                    merchant.CouponType,
                    merchant.DishType,
                    merchant.TimeforOpenBusiness,
                    merchant.TimeforCloseBusiness,
                    //merchant.Wallet,      // 添加钱包信息  
                    // merchant.WalletPassword,
                };

                return Ok(new { data = merchantInfo, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPost]
        [Route("CreateFM")]  // 创建收藏项
        public IActionResult CreateFavouriteMerchant([FromBody] FavoriteMerchantDBDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                FavoriteMerchantDB FM = new FavoriteMerchantDB()
                {
                    UserId = dto.UserId,
                    MerchantId = dto.MerchantId,
                };

                _context.FavoriteMerchants.Add(FM);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { data = FM.MerchantId, msg = "创建成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("getFM")]  // 获取收藏项
        public IActionResult GetFavouriteMerchant(int userId)
        {
            try
            {
                var user = _context.Users.FirstOrDefault(m => m.UserId == userId);

                if (user == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "用户未找到" });
                }

                // 查询用户最喜欢的商家 ID  
                var favoriteMerchants = _context.FavoriteMerchants
                    .Where(fm => fm.UserId == userId)
                    .Select(fm => fm.MerchantId)
                    .ToList();

                if (favoriteMerchants.Count == 0)
                {
                    return Ok(new { data = new List<int>(), msg = "没有找到收藏商家" });
                }

                return Ok(new { data = favoriteMerchants, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }

        }
        [HttpDelete]
        [Route("deleteFM")]  // 删除收藏项
        public IActionResult DeleteFavouriteMerchant(FavoriteMerchantDBDto dto)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的菜品  
                var FM = _context.FavoriteMerchants.FirstOrDefault(d => d.UserId == dto.UserId && d.MerchantId == dto.MerchantId);
                if (FM == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除未找到" });
                }
                // 删除菜品  
                _context.FavoriteMerchants.Remove(FM);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "收藏删除成功" });
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
        [Route("addToShoppingCart")] // 将菜品加入购物车
        public IActionResult addToShoppingCart([FromBody] ShoppingCartDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                ShoppingCartDB CartRecord = new ShoppingCartDB()
                {
                    ShoppingCartId = _userService.AssignCartId(),
                    UserId = dto.UserId,
                    MerchantId = dto.MerchantId,
                    DishId = dto.DishId,
                    DishNum = dto.DishNum,
                };

                // 检查是否已经存在同一用户、同一商家和同一菜品的记录
                var existingCartRecord = _context.ShoppingCarts
                    .FirstOrDefault(cart => cart.UserId == dto.UserId
                                            //&& cart.MerchantId == dto.MerchantId
                                            && cart.DishId == dto.DishId);

                if (existingCartRecord != null)
                {
                    // 如果存在，则将 DishNum 更新
                    existingCartRecord.DishNum += dto.DishNum;
                    _context.ShoppingCarts.Update(existingCartRecord);
                }
                else
                {
                    // 如果不存在，创建新记录
                    _context.ShoppingCarts.Add(CartRecord);
                }

                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();//多表添加才用到
                    return Ok(new { msg = existingCartRecord != null ? "更新成功" : "创建成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "创建失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务  
                return StatusCode(30000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }

        [HttpPut]
        [Route("decrementDishInCart")] // 从购物车中减少一个菜品
        public IActionResult decrementDishInCart([FromBody] ShoppingCartDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务
            try
            {
                // 查找是否存在同一用户、同一商家和同一菜品的记录
                var existingCartRecord = _context.ShoppingCarts
                    .FirstOrDefault(cart => cart.UserId == dto.UserId
                                            //&& cart.MerchantId == dto.MerchantId
                                            && cart.DishId == dto.DishId);

                if (existingCartRecord != null)
                {
                    if (existingCartRecord.DishNum > 1)
                    {
                        // 如果 DishNum 大于 1，减少 DishNum
                        existingCartRecord.DishNum -= 1;
                        _context.ShoppingCarts.Update(existingCartRecord);
                    }
                    else
                    {
                        // 如果 DishNum 为 1，直接删除该记录
                        _context.ShoppingCarts.Remove(existingCartRecord);
                    }

                    var result = _context.SaveChanges();

                    if (result > 0)
                    {
                        tran.Commit(); // 提交事务
                        return Ok(new { msg = "操作成功" });
                    }
                    else
                    {
                        return StatusCode(20000, new { errorCode = 20000, msg = "操作失败" });
                    }
                }
                else
                {
                    return NotFound(new { errorCode = 40000, msg = "未找到相应的购物车记录" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务
                return StatusCode(30000, new { errorCode = 30000, msg = $"操作异常: {ex.Message}" });
            }
        }

        [HttpDelete]
        [Route("removeFromShoppingCart")] // 从购物车中删除菜品
        public IActionResult removeFromShoppingCart([FromBody] ShoppingCartDto dto)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务
            try
            {
                // 查找是否存在同一用户、同一商家和同一菜品的记录
                var existingCartRecord = _context.ShoppingCarts
                    .FirstOrDefault(cart => cart.UserId == dto.UserId
                                            //&& cart.MerchantId == dto.MerchantId
                                            && cart.DishId == dto.DishId);

                if (existingCartRecord != null)
                {
                    // 如果存在该记录，删除该记录
                    _context.ShoppingCarts.Remove(existingCartRecord);
                    var result = _context.SaveChanges();

                    if (result > 0)
                    {
                        tran.Commit(); // 提交事务
                        return Ok(new { msg = "删除成功" });
                    }
                    else
                    {
                        return StatusCode(20000, new { errorCode = 20000, msg = "删除失败" });
                    }
                }
                else
                {
                    return NotFound(new { errorCode = 40000, msg = "未找到相应的购物车记录" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback(); // 回滚事务
                return StatusCode(30000, new { errorCode = 30000, msg = $"删除异常: {ex.Message}" });
            }
        }

        [HttpGet]
        [Route("getShoppingCartItems")]  // 获取购物车中的所有物品
        public IActionResult getShoppingCartItems(int userId)
        {
            try
            {
                // 检查用户是否存在
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);

                if (user == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "用户未找到" });
                }

                // 查询用户购物车中的所有物品，并关联菜品信息
                var cartItems = _context.ShoppingCarts
                    .Where(cart => cart.UserId == userId)
                    .Include(cart => cart.DishDB)  // 通过 Include 方法加载 DishDB 相关数据
                    .Select(cart => new
                    {
                        cart.ShoppingCartId,
                        cart.MerchantId,
                        cart.DishId,
                        cart.DishNum,
                        DishName = cart.DishDB.DishName,  // 获取菜品名称
                        DishPrice = cart.DishDB.DishPrice, // 获取菜品价格
                        ImageUrl = cart.DishDB.ImageUrl,   // 获取菜品图片URL
                    })
                    .ToList();
                var merchantInfo = _context.Merchants
                .Where(m => cartItems.Select(ci => ci.MerchantId).Contains(m.MerchantId))
                .ToDictionary(m => m.MerchantId, m => m.MerchantName);

                var result = cartItems.Select(cart => new
                {
                    cart.ShoppingCartId,
                    cart.MerchantId,
                    MerchantName = merchantInfo[cart.MerchantId], // 从 merchantInfo 中获取商家名称
                    cart.DishId,
                    cart.DishNum,
                    cart.DishName,
                    cart.DishPrice,
                    cart.ImageUrl
                }).ToList();

                if (result.Count == 0)
                {
                    return Ok(new { data = new List<object>(), msg = "购物车为空" });
                }

                return Ok(new { data = result, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
    }
}
