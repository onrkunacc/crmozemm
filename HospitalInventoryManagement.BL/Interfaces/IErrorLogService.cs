using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HospitalInventoryManagement.Data.Models;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IErrorLogService
    {
        void LogError(string message, string stackTrace);
        List<ErrorLogs> GetAllLogs();
    }
}
