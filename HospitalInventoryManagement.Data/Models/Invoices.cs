using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Invoices
    {
        public int Id { get; set; } // Primary Key
        public string Donemi { get; set; } // Fatura Dönemi (örn: 2024)
        public int Ay { get; set; } //  Fatura Ayı (örn: 1 = Ocak, 2 = Şubat)
        public decimal Tutar { get; set; } // Fatura Tutarı
        public DateTime? KapanisTarihi { get; set; } // Opsiyonel
        public int CariId { get; set; } // Foreign Key
        public Cariler Cari { get; set; } // Navigation Property
    }
}
