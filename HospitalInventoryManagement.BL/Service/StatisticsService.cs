using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.DTOs;
using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Service
{

    public class StatisticsService : IStatisticsService
    {
        private readonly ApplicationDbContext _context;

        public StatisticsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void AddMonthlyStatistics(int hospitalId, DateTime month, int testCount, string testName)
        {
            var statistics = new MonthlyStatistics
            {
                HospitalID = hospitalId,
                Month = month,
                TestCount = testCount,
                TestName = testName
            };
            _context.MonthlyStatistics.Add(statistics);
            _context.SaveChanges();
        }

        public List<StatisticsDTO> GetStatisticsByHospital(int hospitalId)
        {
            return _context.MonthlyStatistics
                .Where(s => s.HospitalID == hospitalId)
                .Select(s => new StatisticsDTO
                {
                    HospitalID = s.HospitalID,
                    HospitalName = s.Hospital.HospitalName,
                    Month = s.Month,
                    TestName = s.TestName,
                    TestCount = s.TestCount
                })
                .ToList();
        }

        public List<StatisticsDTO> GetStatisticsByDateRange(int hospitalId, DateTime startDate, DateTime endDate)
        {
            return _context.MonthlyStatistics
                .Where(s => s.HospitalID == hospitalId && s.Month >= startDate && s.Month <= endDate)
                .Select(s => new StatisticsDTO
                {
                    HospitalID = s.HospitalID,
                    HospitalName = s.Hospital.HospitalName,
                    Month = s.Month,
                    TestName = s.TestName,
                    TestCount = s.TestCount
                })
                .ToList();
        }

        public List<StatisticsDTO> GetAllStatistics()
        {
            return _context.MonthlyStatistics
                .Select(s => new StatisticsDTO
                {
                    HospitalID = s.HospitalID,
                    HospitalName = s.Hospital.HospitalName,
                    Month = s.Month,
                    TestName = s.TestName,
                    TestCount = s.TestCount
                })
                .ToList();
        }
    }
}
