using HospitalInventoryManagement.Data.DTOs;
using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IStatisticsService
    {
        void AddMonthlyStatistics(int hospitalId, DateTime month, int testCount, string testName);
        List<StatisticsDTO> GetStatisticsByHospital(int hospitalId);
        List<StatisticsDTO> GetStatisticsByDateRange(int hospitalId, DateTime startDate, DateTime endDate);
        List<StatisticsDTO> GetAllStatistics();

    }
}
