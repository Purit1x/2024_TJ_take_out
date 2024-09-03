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
using Quartz.Spi; // ȷ��������������ռ���ʹ�� IJobFactory  
using takeout_tj.Job;
using Quartz.AspNetCore;
//using Quartz.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

//ȫ��ע�� --����������Ŀ�����еķ�������Ч��
builder.Services.AddControllers(opt =>
{
    opt.Filters.Add<CustomExceptionFilterAttribute>();
    opt.Conventions.Insert(0, new RouteConvention(new RouteAttribute("api/")));
})
//���������������
.AddJsonOptions(options =>
{
    //ȡ��Unicode����
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
    //��ʽ������ʱ���ʽ
    // options.JsonSerializerOptions.Converters.Add(new DatetimeJsonConverter("yyyy-MM-dd HH:mm:ss"));
    //���ݸ�ʽ����ĸСд
    // options.JsonSerializerOptions.PropertyNamingPolicy =JsonNamingPolicy.CamelCase;
    //���ݸ�ʽԭ�����
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    //���Կ�ֵ
    options.JsonSerializerOptions.IgnoreNullValues = true;
    //����������
    options.JsonSerializerOptions.AllowTrailingCommas = true;
    //�����л����������������Ƿ�ʹ�ò����ִ�Сд�ıȽ�
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = false;
})
// Microsoft.AspNetCore.Mvc.NewtonsoftJson;
// ���Json ת������,���磺ǰ̨��ʱ���̨ת������ʱ�м�ո���Ҫ��ΪT
// ��[JsonProperty("role_id")] ����Ҫ��[JsonPropertyName("id")] 
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
//builder.Services.AddScoped<CouponService>(); // ���ʵ���������
// ��Program.csע�������Ĳ�ָ�����ݿ������ַ���
// builder.Services.AddDbContext<ModelContext>(option => option.UseOracle(builder.Configuration.GetConnectionString("WaiMaiDBCon")));

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "����ƽ̨����ϵͳ����",
        Description = "������swagger����"
    });
});
// ���Ի����򿪷�������, �κο�������ͨ��
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowAllOrigins", policy =>
    {
        policy.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
// ���Quartz����  
builder.Services.AddQuartz(q =>
{
    q.UseSimpleTypeLoader();
    q.UseInMemoryStore(); // ʹ���ڴ�洢�������־ô洢  
    q.ScheduleJob<OrderCleanupJob>(trigger => trigger
        .WithIdentity("OrderCleanupJobTrigger")
        .StartNow()
        .WithSimpleSchedule(x => x
            .WithInterval(TimeSpan.FromSeconds(60)) // ÿ60��ִ��һ��  
            .RepeatForever()));
});

// ��� Quartz ����  
builder.Services.AddQuartzServer(options =>
{
    options.WaitForJobsToComplete = true; // ��ѡ, �ȴ���ҵ���  
});

builder.Services.AddTransient<OrderCleanupService>();
builder.Services.AddLogging();
builder.Logging.ClearProviders();
builder.Logging.AddConsole(); // ���������̨  
builder.Logging.AddDebug(); // ���������Ϣ 

var app = builder.Build();

// �������
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

// ʹ uploads �ļ��ж���ɷ���  
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "uploads")),
    RequestPath = "/uploads"
});

app.MapControllers();

app.Run();



