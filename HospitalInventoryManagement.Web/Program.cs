using HospitalInventoryManagement.BL.Extensions;
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.BL.Service;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant� dizgesini al
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// DbContext ayar�
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Identity ayarlar�
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()  // Rollerle birlikte kimlik do�rulama
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();  // �ifre s�f�rlama ve e-posta do�rulama gibi i�lemler i�in token sa�lay�c�

// Service ve Interface eklemeleri
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStockService, StockService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IBarcodeService, BarcodeService>();
builder.Services.AddScoped<IApiService, ApiService>();  
builder.Services.AddScoped<IErrorLogService, ErrorLogService>();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "HospitalInventory";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // �erez s�resi, iste�e ba�l�
    options.SlidingExpiration = false; // �erez s�resi dolunca otomatik oturum kapatma
    options.Cookie.HttpOnly = true;


    options.LoginPath = "/Account/Login"; // Giri� sayfas� yolu
    options.LogoutPath = "/Account/Logout"; // ��k�� sayfas� yolu
    options.AccessDeniedPath = "/Account/AccessDenied"; // Eri�im engellendi�inde y�nlendirilecek sayfa
});

// MVC ve Razor Pages deste�i
builder.Services.AddControllersWithViews();

builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequiredUniqueChars = 1;
});

var app = builder.Build();

// Rolleri seed eden bir fonksiyon eklemek
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    await SeedRolesAndAdminsAsync(roleManager, userManager);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<ExceptionMiddleware>();

// Kimlik do�rulama ve yetkilendirme middleware'lerini ekle
app.UseAuthentication();
app.UseAuthorization();

// MVC route ayar�
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Rolleri ve admin kullan�c�y� seed eden fonksiyon
async Task SeedRolesAndAdminsAsync(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
{
    // Rolleri olu�tur
    var roles = new[] { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Admin kullan�c� listesi
    var adminUsers = new[]
    {
        new { Email = "ozem@ozemmedikal.com", UserName = "ozem", Password = "ozem123" },
        new { Email = "admin@ozemmedikal.com", UserName = "admin", Password = "admin123" }
    };

    foreach (var admin in adminUsers)
    {
        var adminUser = await userManager.FindByEmailAsync(admin.Email);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = admin.UserName,
                Email = admin.Email,
                EmailConfirmed = true
            };

            var createResult = await userManager.CreateAsync(adminUser, admin.Password);
            if (!createResult.Succeeded)
            {
                foreach (var error in createResult.Errors)
                {
                    Console.WriteLine($"Hata: {error.Description}");
                }
                continue;
            }
        }

        // Admin rol�n� atama
        if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

