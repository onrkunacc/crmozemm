using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalInventoryManagement.Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalInventoryManagement.BL.Extensions
{
    public class SeedUsers
    {
        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // Evrak ve Fatura kullanıcıları
            var users = new[]
            {
                new { Email = "evrak@ozemmedikal.com", Password = "evrak123", Role = "DocumentRole" },
                new { Email = "fatura@ozemmedikal.com", Password = "fatura123", Role = "InvoiceRole" }
            };

            foreach (var userData in users)
            {
                // Kullanıcı zaten varsa, atla
                var user = await userManager.FindByEmailAsync(userData.Email);
                if (user == null)
                {
                    // Yeni kullanıcı oluştur
                    user = new ApplicationUser
                    {
                        UserName = userData.Email.Split('@')[0], // Kullanıcı adı: e-posta öncesi
                        Email = userData.Email,
                        EmailConfirmed = true, // E-posta doğrulandı olarak işaretlenir
                        HospitalID = 0, // HospitalID her kullanıcı için 0
                        IsAdmin = false // IsAdmin her kullanıcı için false
                    };

                    var result = await userManager.CreateAsync(user, userData.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            Console.WriteLine($"Kullanıcı oluşturulamadı: {error.Description}");
                        }
                        continue;
                    }
                }

                // Rol ataması yap
                if (!await userManager.IsInRoleAsync(user, userData.Role))
                {
                    await userManager.AddToRoleAsync(user, userData.Role);
                }
            }
        }
    }
}
