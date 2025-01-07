using System.ComponentModel.DataAnnotations;

namespace HospitalInventoryManagement.Web.Areas.Invoice.Models.ViewModels
{
    public class EditInvoiceViewModel
    {
        public int FaturaId { get; set; }
        public int CariId { get; set; }
        public string CariAdi { get; set; }
        public int Ay { get; set; }
        public int Yil { get; set; }
        [Required]
        [DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
        public decimal Tutar { get; set; }
        public string? Ekler { get; set; }

    }
}
