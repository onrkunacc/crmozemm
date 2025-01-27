using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IFileProccesingService
    {
        string ConvertWordToFormattedHtml(string filePath);
        void UpdateWordDocumentFromHtml(string filePath, string htmlContent);
    }
}
