using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface IApiService
    {

        Task<(string TableHtml, List<int> PageNumbers)> FetchHtmlTableWithPagesAsync(int pageNumber);
        Task LoginAsync(string username, string password);
    }
}
