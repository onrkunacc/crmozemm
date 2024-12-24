using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Context;
using HospitalInventoryManagement.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Service
{
    public class StockService : IStockService
    {
        private readonly ApplicationDbContext _context;

        public StockService(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Stock> GetAllStocks()
        {
            return _context.Stocks.Include(s => s.Product)
                                  .Include(s => s.Hospital)
                                  .ToList();
        }

        public List<Stock> GetStocksByHospital(int hospitalId)
        {
            return _context.Stocks.Include(s => s.Product)
                                  .Include(s=> s.Hospital)
                                  .Where(s => s.HospitalID == hospitalId)
                                  .ToList();
        }

        public void UpdateStock(Stock stock)
        {
            var existingStock = _context.Stocks.FirstOrDefault(s => s.ProductID == stock.ProductID && s.HospitalID == stock.HospitalID);

            if (existingStock != null)
            {
                existingStock.Quantity = stock.Quantity;
                existingStock.ProductID = stock.ProductID;
                existingStock.HospitalID = stock.HospitalID;
                existingStock.ExpiryDate = stock.ExpiryDate;
                existingStock.LastUpdated = stock.LastUpdated;

                _context.SaveChanges();
            }
        }

        public void DeleteStock(int stockId)
        {
            var stock = _context.Stocks.Find(stockId);
            if (stock != null)
            {
                _context.Stocks.Remove(stock);
                _context.SaveChanges();
            }
        }
        public void AddStock(Stock stock)
        {
            _context.Stocks.Add(stock);
            _context.SaveChanges();
        }

        public Stock GetStockById(int id)
        {
            return _context.Stocks
                     .Include(s => s.Product) // Product ilişkisinin dahil edilmesi
                     .Include(s => s.Hospital) // Hospital ilişkisinin dahil edilmesi
                     .FirstOrDefault(s => s.StockID == id);
        }
        public void AddStockTransaction(StockTransaction transaction)
        {
            _context.StockTransactions.Add(transaction);
            _context.SaveChanges();
        }

        public void UpdateStockQuantity(int stockId, int changeQuantity)
        {
            var stock = _context.Stocks.FirstOrDefault(s => s.StockID == stockId);
            if (stock != null)
            {
                stock.Quantity += changeQuantity;
                _context.SaveChanges();
            }
        }

        public Stock GetStockByReferenceNumber(string referenceNumber, int hospitalId)
        {
            return _context.Stocks
                           .Include(s => s.Product)
                           .Include(s => s.Hospital)
                           .FirstOrDefault(s => s.Product.ReferenceNumber == referenceNumber && s.HospitalID == hospitalId);
        }


        public bool ProcessStockTransaction(string referenceNumber, int quantityChange, int hospitalId, string lotNumber = null, DateTime? expiryDate = null)
        {
            // Stoğu referans numarası ve hastane ID'sine göre bul
            var stock = _context.Stocks
                                .Include(s => s.Product)
                                .FirstOrDefault(s => s.Product.ReferenceNumber == referenceNumber && s.HospitalID == hospitalId);

            if (stock == null)
            {
                // Stoğun bulunamaması durumunda yeni stok kaydı oluştur
                stock = new Stock
                {
                    ProductID = _context.Products.FirstOrDefault(p => p.ReferenceNumber == referenceNumber)?.ProductID ?? 0,
                    HospitalID = hospitalId,
                    Quantity = quantityChange, // İlk stok miktarı olarak giriş miktarını ayarla
                    LotNumber = lotNumber,
                    ExpiryDate = expiryDate ?? DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                _context.Stocks.Add(stock); // Yeni stok kaydını ekle
            }
            else
            {
                // Stok çıkışı için miktar yeterliliğini kontrol et
                if (quantityChange < 0 && stock.Quantity + quantityChange < 0)
                {
                    throw new InvalidOperationException("Yetersiz stok miktarı. Çıkış yaparken stok miktarı 0'ın altına düşemez.");
                }

                // Mevcut stok kaydını güncelle
                stock.Quantity += quantityChange;
                stock.LotNumber = lotNumber ?? stock.LotNumber; // Eğer yeni lot numarası varsa güncelle
                stock.ExpiryDate = expiryDate ?? stock.ExpiryDate; // Eğer yeni bir son kullanma tarihi varsa güncelle
                stock.LastUpdated = DateTime.Now;
            }

            // İşlem kaydını `StockTransaction` tablosuna ekle
            var transaction = new StockTransaction
            {
                StockID = stock.StockID,
                ChangeQuantity = quantityChange,
                TransactionDate = DateTime.Now,
                TransactionType = quantityChange > 0 ? "Giriş" : "Çıkış",
                LotNumber = lotNumber, // Lot numarasını işlem kaydına ekliyoruz
                Description = quantityChange > 0 ? "Barkod ile Stok Girişi" : "Barkod ile Stok Çıkışı"
            };

            // İşlem kaydını veritabanına ekle ve tüm değişiklikleri kaydet
            _context.StockTransactions.Add(transaction);
            _context.SaveChanges();

            return true; // İşlem başarılı
        }

        public void AddNewStock(string referenceNumber, string productName, int quantity, int hospitalId, string lotNumber, DateTime expiryDate)
        {
            // Ürün tablosunda mevcutsa ürünü getir veya yeni bir ürün oluştur
            var product = _context.Products.FirstOrDefault(p => p.ReferenceNumber == referenceNumber)
                          ?? new Product { ReferenceNumber = referenceNumber, ProductName = productName };

            // Yeni stok oluştur
            var newStock = new Stock
            {
                Product = product,
                HospitalID = hospitalId,
                Quantity = quantity,
                LotNumber = lotNumber,
                ExpiryDate = expiryDate,
                LastUpdated = DateTime.Now
            };

            _context.Stocks.Add(newStock);
            _context.SaveChanges();
        }
    }
}
