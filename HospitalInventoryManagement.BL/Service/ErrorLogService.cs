using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;

namespace HospitalInventoryManagement.BL.Service
{
    public class ErrorLogService : IErrorLogService
    {
        private readonly ApplicationDbContext _context;

        public ErrorLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void LogError(string message, string stackTrace)
        {
            var errorLog = new ErrorLogs
            {
                ErrorMessage = message,
                StackTrace = stackTrace,
                LogTime = DateTime.Now
            };
            _context.ErrorLogs.Add(errorLog);
            _context.SaveChanges();
        }

        public List<ErrorLogs> GetAllLogs()
        {
            return _context.ErrorLogs.OrderByDescending(e => e.LogTime).ToList();
        }
    }
}
