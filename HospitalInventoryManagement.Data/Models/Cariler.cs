using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class Cariler
    {
        public int Id { get; set; } // Primary Key
        public string CariKodu { get; set; } // Örneğin: 120.01.001
        public string Unvan { get; set; } // Örneğin: DENİZLİ DEVLET HASTANESİ
        public DateTime? KapanisTarihi { get; set; } // Opsiyonel
        public string TeklifNo { get; set; } // Teklif Numarası
        public string IhaleNo { get; set; } // İhale Numarası
        public bool Tevkifat { get; set; } // Evet/Hayır
        public int CariGrubuId { get; set; } // Foreign Key
        public CariGruplari CariGrubu { get; set; } // Navigation Property

        public ICollection<Invoices> Invoices { get; set; } // Bir carinin birden fazla faturası olabilir
    }
}
