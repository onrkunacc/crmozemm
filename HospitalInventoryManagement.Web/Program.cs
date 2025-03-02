using HospitalInventoryManagement.BL.Extensions;
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.BL.Service;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantı dizgesini al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext ayarı
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity ayarları
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()  // Rollerle birlikte kimlik doğrulama
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();  



// Service ve Interface eklemeleri
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();
builder.Services.AddScoped<IFileProccesingService,FileProcessingService>();


builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "auth-cookie";
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
        options.Cookie.SameSite = SameSiteMode.Lax; 
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); 
        options.SlidingExpiration = true; 
    });

// MVC ve Razor Pages desteği
builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;

    options.User.RequireUniqueEmail = true; 
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("DocumentAccess", policy =>
        policy.RequireRole("DocumentRole", "Admin"));
});

var app = builder.Build();

// Rolleri seed eden bir fonksiyon eklemek
using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    
}


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();



app.UseAuthentication();
app.UseAuthorization();


app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");



app.Run();


