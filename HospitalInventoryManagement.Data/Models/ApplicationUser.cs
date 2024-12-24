using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HospitalInventoryManagement.Data.Models
{
    public class ApplicationUser : IdentityUser
    {
        public int HospitalID { get; set; }
        public bool IsAdmin { get; set; }   
    }
}
