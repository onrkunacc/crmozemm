using HospitalInventoryManagement.BL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ErrorLogController : Controller
    {
        private readonly IErrorLogService _errorLogService;

        public ErrorLogController(IErrorLogService errorLogService)
        {
            _errorLogService = errorLogService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var logs = _errorLogService.GetAllLogs();
            return View(logs);
        }

    }
}
