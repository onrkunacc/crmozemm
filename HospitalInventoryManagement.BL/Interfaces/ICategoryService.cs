using HospitalInventoryManagement.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.BL.Interfaces
{
    public interface ICategoryService
    {
        List<Category> GetAllCategories();
        void AddCategory(Category category);
        void DeleteCategory(int categoryId);
        void UpdateCategory(Category category);

    }
}
