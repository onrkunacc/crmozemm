using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IBarcodeService
    {
        bool ProcessBarcode(string barcode,int hospitalId);
    }
}
