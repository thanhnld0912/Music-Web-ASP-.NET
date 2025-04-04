using Microsoft.AspNetCore.Session;
using Microsoft.EntityFrameworkCore;
using MusicWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Lấy Connection String từ appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Đăng ký DbContext vào DI container
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllersWithViews();

// Thêm dịch vụ Session
builder.Services.AddDistributedMemoryCache(); // Sử dụng bộ nhớ tạm
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session timeout trong 30 phút
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});


// Thêm HttpContextAccessor để truy cập Session từ view
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Cấu hình pipeline
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Sử dụng Session trước Authorization
app.UseSession();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.Run();