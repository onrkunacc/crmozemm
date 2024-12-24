using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IStockService
    {
        List<Stock> GetAllStocks();
        List<Stock> GetStocksByHospital(int hospitalId);
        Stock GetStockById(int id); 
        void UpdateStock(Stock stock);
        void DeleteStock(int stockId);
        void AddStock(Stock stock);
        void AddStockTransaction(StockTransaction transaction);
        void UpdateStockQuantity(int stockId, int changeQuantity);
        Stock GetStockByReferenceNumber(string referenceNumber, int hospitalId);
        bool ProcessStockTransaction(string referenceNumber, int quantityChange, int hospitalId, string lotNumber = null, DateTime? expiryDate = null);
        void AddNewStock(string referenceNumber, string productName, int quantity, int hospitalId, string lotNumber, DateTime expiryDate);
    }
}
