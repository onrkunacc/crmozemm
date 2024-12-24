using HospitalInventoryManagement.BL.Extensions;
using HospitalInventoryManagement.BL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Service
{
    public class BarcodeService : IBarcodeService
    {
        private readonly IStockService _stockService;

        public BarcodeService(IStockService stockService)
        {
            _stockService = stockService;
        }

        public bool ProcessBarcode(string barcode,int hospitalId)
        {
            // Barkodu ayrıştır
            var barcodeData = BarcodeParser.Parse(barcode);

            if (barcodeData.ReferenceNumber == null || barcodeData.Quantity == 0)
            {
                return false; // Barkoddan veri ayrıştırılamadı
            }

            // Ürünü referans numarasına ve hastane ID'sine göre bul
            var stock = _stockService.GetStockByReferenceNumber(barcodeData.ReferenceNumber, hospitalId);

            if (stock == null)
            {
                return false; // Stok bulunamadı
            }

            // Stok giriş/çıkış işlemi (örneğin, Quantity pozitifse giriş, negatifse çıkış)
            _stockService.UpdateStockQuantity(stock.StockID, barcodeData.Quantity);

            return true; // İşlem başarılı
        }
    }
}
