using Microsoft.EntityFrameworkCore;
using takeout_tj.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add Entity Framework Core services for Oracle database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	options.UseOracle(
		builder.Configuration.GetConnectionString("DefaultConnection"),
		o => o.MigrationsAssembly("takeout_tj"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
