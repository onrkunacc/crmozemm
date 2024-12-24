using HospitalInventoryManagement.Data.DTOs;
using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Extensions
{
    public static class DtoMapper
    {
        public static StockDTO ToStockDTO(Stock stock)
        {
            return new StockDTO
            {
                StockID = stock.StockID,
                ProductID = stock.ProductID,
                Quantity = stock.Quantity,
                ExpiryDate = stock.ExpiryDate,
                ProductName = stock.Product?.ProductName  // Product varsa ProductName'i al
            };
        }

        public static HospitalDTO ToHospitalDTO(Hospital hospital)
        {
            return new HospitalDTO
            {
                HospitalID = hospital.HospitalID,
                HospitalName = hospital.HospitalName,
                Stocks = hospital.Stocks?.Select(ToStockDTO).ToList()  // Stocks'i DTO'ya çevir
            };
        }
    }
}
