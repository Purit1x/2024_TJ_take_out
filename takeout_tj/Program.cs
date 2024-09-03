using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using takeout_tj.Data;
using takeout_tj.Utility;
using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.FileProviders;
using takeout_tj.Service;
using Quartz;
using Quartz.Impl;
using Quartz.Spi; // 确保引入这个命名空间以使用 IJobFactory  
using takeout_tj.Job;
using Quartz.AspNetCore;
//using Quartz.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

//全局注册 --对于整个项目中所有的方法都生效的
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<CustomExceptionFilterAttribute>();
    opt.Conventions.Insert(0, new RouteConvention(new RouteAttribute("api/")));
})
//解决中文乱码问题
.AddJsonOptions(options =>
{
    //取消Unicode编码
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    //格式化日期时间格式
    // options.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
    //数据格式首字母小写
    // options.JsonSerializerOptions.PropertyNamingPolicy =JsonNamingPolicy.CamelCase;
    //数据格式原样输出
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    //忽略空值
    options.JsonSerializerOptions.IgnoreNullValues = true;
    //允许额外符号
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    //反序列化过程中属性名称是否使用不区分大小写的比较
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
})
// Microsoft.AspNetCore.Mvc.NewtonsoftJson;
// 解决Json 转换问题,例如：前台传时间后台转换对象时中间空格需要变为T
// 用[JsonProperty("role_id")] 而不要用[JsonPropertyName("id")] 
.AddNewtonsoftJson();

// Add services to the container.
// builder.Services.AddControllersWithViews();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework Core services for Oracle database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseOracle(
		builder.Configuration.GetConnectionString("WaiMaiDBCon"),
		o => o.MigrationsAssembly("takeout_tj"));
});
//builder.Services.AddScoped<CouponService>(); // 合适的生命周期
// 在Program.cs注册上下文并指定数据库连接字符串
// builder.Services.AddDbContext<ModelContext>(option => option.UseOracle(builder.Configuration.GetConnectionString("WaiMaiDBCon")));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "外卖平台管理系统测试",
        Description = "这里是swagger测试"
    });
});
// 测试环境或开发环境下, 任何跨域请求都通过
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
// 添加Quartz服务  
builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore(); // 使用内存存储或其他持久存储  
    q.ScheduleJob<OrderCleanupJob>(trigger => trigger
        .WithIdentity("OrderCleanupJobTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromSeconds(60)) // 每60秒执行一次  
            .RepeatForever()));
});

// 添加 Quartz 服务  
builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true; // 可选, 等待作业完成  
});

builder.Services.AddTransient<OrderCleanupService>();
builder.Services.AddLogging();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // 输出到控制台  
builder.Logging.AddDebug(); // 输出调试信息 

var app = builder.Build();

// 允许跨域
app.UseCors("AllowAllOrigins");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

// 使 uploads 文件夹对外可访问  
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();

app.Run();



