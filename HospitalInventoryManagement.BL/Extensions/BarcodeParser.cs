using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Extensions
{
    public static class BarcodeParser
    {
        // Barkod verisini ayrıştıran metod
        public static BarcodeData Parse(string barcode)
        {
            if (string.IsNullOrEmpty(barcode))
                throw new ArgumentException("Barkod boş veya geçersiz.");

            try
            {
                BarcodeData data = new BarcodeData();

                // Barkod verilerini ayrıştır
                data.ExpiryDate = ParseExpiryDate(barcode);
                data.LotNumber = ParseLotNumber(barcode);
                data.ReferenceNumber = ParseReferenceNumber(barcode);


                // Referans numarası bulunamazsa hata fırlat
                if (string.IsNullOrEmpty(data.ReferenceNumber))
                    throw new Exception("Referans numarası barkodda bulunamadı.");

                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
                throw;
            }
        }

        // 240 kodunu kullanarak referans numarasını ayrıştırır
        private static string ParseReferenceNumber(string barcode)
        {
            int index = barcode.IndexOf("240");
            if (index >= 0 && barcode.Length > index + 3)
            {
                // "240" kodundan sonra gelen kısmı al
                string rawRefNumber = barcode.Substring(index + 3);

                // Başlangıçtaki tüm boşluk, "0" ve "*" karakterlerini temizleyelim
                rawRefNumber = Regex.Replace(rawRefNumber, @"^[\s0*]+", "");


                // Şimdi ilk 6 karakteri alıyoruz, fakat sadece alfanumerik olanları topluyoruz
                var refNumberBuilder = new StringBuilder();
                foreach (char ch in rawRefNumber)
                {
                    if (char.IsLetterOrDigit(ch))
                    {
                        refNumberBuilder.Append(ch);
                    }

                    // 6 karaktere ulaştıysak döngüyü kır
                    if (refNumberBuilder.Length == 6)
                        break;
                }

                string refNumber = refNumberBuilder.ToString();

                return refNumber;
            }
            return null;
        }

        // 17 kodunu kullanarak son kullanma tarihini ayrıştırır
        private static DateTime? ParseExpiryDate(string barcode)
        {
            int index = barcode.IndexOf("17");
            if (index >= 0 && barcode.Length > index + 8)
            {
                // "17" kodundan sonra gelen kısmı al
                string rawExpiryDate = barcode.Substring(index + 2);

                // Başlangıçtaki boşluk ve "0" karakterlerini temizleyin
                rawExpiryDate = Regex.Replace(rawExpiryDate, @"^[\s0]+", "");

                // İlk 6 karakteri alıp tarihi parse etme
                string dateStr = new string(rawExpiryDate.Take(6).ToArray());
                if (DateTime.TryParseExact(dateStr, "yyMMdd", null, System.Globalization.DateTimeStyles.None, out DateTime expiryDate))
                {
                    return expiryDate;
                }

                Console.WriteLine("Geçersiz son kullanma tarihi formatı.");
            }
            return null;
        }

        // 10 kodunu kullanarak lot numarasını ayrıştırır
        private static string ParseLotNumber(string barcode)
        {
            if (string.IsNullOrWhiteSpace(barcode))
                throw new Exception("Barkod boş olamaz.");

            // İlk olarak "17" kodunun sonunu bul
            int expiryDateEndIndex = barcode.IndexOf("17");
            if (expiryDateEndIndex >= 0 && barcode.Length > expiryDateEndIndex + 8)
            {
                // Tarih formatının bitiş noktası
                expiryDateEndIndex += 8; // "17" + "yyMMdd" = 8 karakter
            }
            else
            {
                expiryDateEndIndex = 0; // Eğer "17" yoksa baştan başla
            }

            // "10" kodunu, "17" kodunun sonundan itibaren aramaya başla
            int lotStartIndex = barcode.IndexOf("10", expiryDateEndIndex);
            if (lotStartIndex < 0)
                throw new Exception("Barkodda '10' kodu bulunamadı.");

            // "240" kodunun pozisyonunu bul
            int lotEndIndex = barcode.IndexOf("240", lotStartIndex);
            if (lotEndIndex < 0)
            {
                lotEndIndex = barcode.Length; // Eğer "240" kodu yoksa barkodun sonuna kadar al
            }

            // "10" kodundan sonra gelen kısmı "240" koduna kadar kes
            string rawLotNumber = barcode.Substring(lotStartIndex + 2, lotEndIndex - (lotStartIndex + 2));

            // Alfanümerik karakterleri filtrele
            string lotNumber = new string(rawLotNumber.Where(char.IsLetterOrDigit).ToArray());

            // Geçerli bir lot numarası olmalı
            if (string.IsNullOrEmpty(lotNumber) || lotNumber.Length < 3)
            {
                throw new Exception($"Geçersiz lot numarası: '{lotNumber}' en az 3 karakter olmalı.");
            }

            return lotNumber;
        }
    }
    public class BarcodeData
    {
        public string ReferenceNumber { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public string LotNumber { get; set; }
        public int Quantity { get; set; }
    }
}
