using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalInventoryManagement.BL.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalInventoryManagement.BL.Extensions
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider; // Scoped servis için IServiceProvider kullanıyoruz.

        public ExceptionMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                // Scoped service'e ulaşmak için bir scope oluşturun.
                using (var scope = _serviceProvider.CreateScope())
                {
                    var errorLogService = scope.ServiceProvider.GetRequiredService<IErrorLogService>();
                    errorLogService.LogError(ex.Message, ex.StackTrace);
                }
                throw; // Hatanın üst katmanlara iletilmesi
            }
        }
    }
}
