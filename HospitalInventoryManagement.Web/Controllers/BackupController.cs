using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace HospitalInventoryManagement.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class BackupController : Controller
    {
        private readonly IConfiguration _configuration;

        public BackupController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View(); // Backup işlemi için buton olan bir View
        }

        [HttpPost]
        public IActionResult BackupDatabase()
        {
            try
            {
                var connectionString = _configuration.GetConnectionString("DefaultConnection");
                var backupFolderPath = "C:\\Backups";

                // Dizin yoksa oluştur
                if (!Directory.Exists(backupFolderPath))
                {
                    Directory.CreateDirectory(backupFolderPath);
                }

                var backupFilePath = $"{backupFolderPath}\\crmozembackup_{DateTime.Now:yyyyMMdd}.bak";

                using (var connection = new SqlConnection(connectionString))
                {
                    var commandText = $@"
                    BACKUP DATABASE [crmozemm2_]
                    TO DISK = '{backupFilePath}'
                    WITH INIT, STATS = 10";

                    using (var command = new SqlCommand(commandText, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

                TempData["SuccessMessage"] = $"Veritabanı yedekleme işlemi başarıyla tamamlandı: {backupFilePath}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Yedekleme işlemi sırasında hata oluştu: {ex.Message}";
            }

            // Yedekleme işlemi sonucu gösterimi için bir View'e yönlendir
            return RedirectToAction("BackupResult");
        }

        [HttpGet]
        public IActionResult BackupResult()
        {
            return View(); // Backup sonucu mesajlarını göstermek için bir View
        }
    }
}
