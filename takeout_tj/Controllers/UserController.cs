using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;
using takeout_tj.DTO;
using takeout_tj.Models.Merchant;
using takeout_tj.Models.Platform;
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

        [HttpPut]
        [Route("withdraw")]
        public IActionResult WalletWithdraw(int userId, int withdrawMoney)
        {
            var tran = _context.Database.BeginTransaction(); // 开始事务 
            try
            {
                var user = _context.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null)
                {
                    return NotFound(new { errorCode = 404, msg = "用户未找到" });
                }
                if (withdrawMoney == 0)
                {
                    tran.Commit();
                    return Ok(new { data = user.Wallet, msg = "用户信息更新成功" });
                }
                user.Wallet = user.Wallet - withdrawMoney;
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit(); // 提交事务  
                    return Ok(new { data = user.Wallet, msg = "用户信息更新成功" });
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
        [Route("CreateFM")]
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
        [Route("getFM")]
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
        [Route("deleteFM")]
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
        [HttpPost]
        [Route("submitAddress")]
        public IActionResult SubmitAddress(AddressDto addressDto)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            var user = _context.Users.Find(addressDto.UserId);
            if (user == null)
            {
                return StatusCode(404, new { errorCode = 404, msg = "未找到该用户" });
            }
            try
            {
                // 创建新的地址对象并保存到数据库
                var newAddress = new UserAddressDB()
                {
                    UserId = addressDto.UserId,
                    UserAddress = addressDto.Address,
                    HouseNumber = addressDto.HouseNumber,
                    ContactName = addressDto.ContactName,
                    PhoneNumber = addressDto.PhoneNumber,
                    AddressId = _userService.AssignAddressId(),
                };
                _context.UserAddresses.Add(newAddress);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { data = newAddress.AddressId, msg = "地址提交成功" });
                }
                else
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "地址提交失败" });
                }
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"提交异常: {ex.Message}", details = ex.InnerException?.Message });
            }
        }

        [HttpDelete]
        [Route("deleteAddress")]
        public IActionResult DeleteAddress(int addressId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 查询要删除的地址  
                var address = _context.UserAddresses.FirstOrDefault(d => d.AddressId == addressId);
                if (address == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "删除未找到" });
                }
                // 删除地址  
                _context.UserAddresses.Remove(address);
                var result = _context.SaveChanges();

                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new { msg = "地址删除成功" });
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
        [Route("getAddress")]
        public IActionResult GetAddress(int userId)
        {
            try
            {
                // 获取用户的所有地址
                var addresses = _context.UserAddresses.Where(a => a.UserId == userId).ToList();
                if (addresses == null || addresses.Count == 0)
                {
                    return NotFound(new { errorCode = 404, msg = "未找到地址" });
                }

                return Ok(addresses);
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = $"获取地址异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("couponList")]
        public IActionResult GetAvailableCoupons()  // 查询所有上架的优惠券  
        {
            try
            {
                // 获取所有 IsOnShelves = 1 的优惠券  
                var coupons = _context.Coupons
                    .Where(m => m.IsOnShelves == 1)
                    .Select(coupon => new
                    {
                        coupon.CouponId,
                        coupon.CouponName,
                        coupon.CouponValue,
                        coupon.CouponPrice,
                        coupon.CouponType,
                        coupon.MinPrice,
                        coupon.PeriodOfValidity,
                        //coupon.QuantitySold,
                        //coupon.IsOnShelves
                    }).ToList();

                if (coupons.Count == 0)
                {
                    return Ok(new { errorCode = 20000, msg = "未找到上架的优惠券" });
                }

                return Ok(new { data = coupons, msg = "获取成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = ex.Message });
            }
        }
        [HttpPost]
        [Route("createCpPurchase")]
        public IActionResult CreateCouponPurchase(CouponPurchaseDto dto)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                if(dto.PurchasingAmount==0)
                {
                    return StatusCode(5000, new { errorCode = 5000, msg = $"购买优惠券数量为零:" });
                }
                var user = _context.Users.FirstOrDefault(d => d.UserId == dto.UserId);
                var coupon = _context.Coupons.FirstOrDefault(d => d.CouponId == dto.CouponId);
                if (user == null || coupon == null)
                {
                    return NotFound(new { errorCode = 404, msg = "未找到用户或优惠券" });
                }
                if(coupon.IsOnShelves==0)
                {
                    return StatusCode(10000, new { errorCode = 10000, msg = $"优惠券已下架:" });
                }
                var wallet = user.Wallet - dto.PurchasingAmount * coupon.CouponPrice;
                if(wallet < 0)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = $"余额不足:" });
                }
                user.Wallet = wallet;
                coupon.QuantitySold = coupon.QuantitySold + dto.PurchasingAmount;
                CouponPurchaseDB couponPurchase = new CouponPurchaseDB()
                {
                    CouponPurchaseId=_userService.AssignCouponPurchaseId(),
                    PurchasingTimestamp=dto.PurchasingTimestamp,
                    CouponId=dto.CouponId,
                    UserId=dto.UserId,
                    PurchasingAmount=dto.PurchasingAmount,
                };
                _context.CouponPurchases.Add(couponPurchase);
                // 计算过期时间  
                DateTime expirationDate = dto.PurchasingTimestamp.AddDays(coupon.PeriodOfValidity);
                UserCouponDB userCoupon = new UserCouponDB()
                {
                    UserId = dto.UserId,
                    CouponId = dto.CouponId,
                    AmountOwned = dto.PurchasingAmount,
                    ExpirationDate=expirationDate,
                };
                _context.UserCoupons.Add(userCoupon);
                var result = _context.SaveChanges();
                if (result > 0)
                {
                    tran.Commit();
                    return Ok(new {  msg = "创建成功" });
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
        [HttpGet]
        [Route("getUserCoupons")]
        public IActionResult GetUserCoupons(int userId)
        {
            var tran = _context.Database.BeginTransaction();  // 开启一个事务  
            try
            {
                // 1. 删除过期的优惠券  
                var now = DateTime.UtcNow; // 获取当前时间  
                var expiredCoupons = _context.UserCoupons
                    .Where(c => c.ExpirationDate < now)
                    .ToList(); // 同步查询过期的优惠券  

                if (expiredCoupons.Any())
                {
                    _context.UserCoupons.RemoveRange(expiredCoupons); // 删除过期的优惠券  
                    _context.SaveChanges(); // 保存更改  
                }

                // 2. 查询指定用户的所有优惠券  
                var userCoupons = _context.UserCoupons
                    .Include(uc => uc.CouponDB) // 包含优惠券详细信息  
                    .Where(uc => uc.UserId == userId)
                    .ToList(); // 同步查询用户的优惠券  

                tran.Commit();
                // 3. 返回结果  
                return Ok(new { data=userCoupons,msg = "创建成功" });
            }
            catch (Exception ex)
            {
                tran.Rollback();
                return StatusCode(30000, new { errorCode = 30000, msg = $"创建异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("getCouponInfo")]
        public IActionResult GetCouponInfo(int couponId)
        {
            try
            {
                var coupon = _context.Coupons.FirstOrDefault(m => m.CouponId == couponId);

                if (coupon == null)
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "优惠券未找到" });
                }
                var res = new
                {
                    coupon.CouponId,
                    coupon.CouponName,
                    coupon.CouponValue,
                    coupon.CouponPrice,
                    coupon.CouponType,
                    coupon.MinPrice,
                    coupon.PeriodOfValidity,
                };
                return Ok(new { data = res, msg = "查找成功" });

            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = $"查找异常: {ex.Message}" });
            }
        }
        [HttpGet]
        [Route("getAllCP")]
        public IActionResult GetAllCouponPurchasesByUser(int userId)
        {
            try
            {
                // 使用 LINQ 查询根据 UserId 获取所有的 CouponPurchaseId  
                var couponPurchases = _context.CouponPurchases
                    .Where(cp => cp.UserId == userId)
                    .Select(cp=>new
                    {
                        cp.CouponPurchaseId,  
                        cp.CouponId,  
                        cp.PurchasingTimestamp,
                        cp.PurchasingAmount
                    })  
                    .ToList();

                if (couponPurchases == null || !couponPurchases.Any())
                {
                    return StatusCode(20000, new { errorCode = 20000, msg = "没有找到相关的优惠券购买记录" });
                }

                return Ok(new { data = couponPurchases, msg = "查找成功" });
            }
            catch (Exception ex)
            {
                return StatusCode(30000, new { errorCode = 30000, msg = $"查找异常: {ex.Message}" });
            }
        }
    }
}
