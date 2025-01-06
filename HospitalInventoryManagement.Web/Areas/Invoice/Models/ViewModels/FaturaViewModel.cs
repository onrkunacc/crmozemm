﻿namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class FaturaViewModel
    {
        public string AyAdi { get; set; }
        public int AyIndex { get; set; }
        public decimal? Tutar { get; set; }
        public bool FaturaVarMi { get; set; }
        public int? FaturaId { get; set; } // Düzenleme için
    }
}
