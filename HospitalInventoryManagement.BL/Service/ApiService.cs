using HospitalInventoryManagement.BL.Interfaces;
using HospitalInventoryManagement.Data.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace HospitalInventoryManagement.BL.Service
{
    public class ApiService : IApiService
    {
        private readonly HttpClient client;
        private readonly HttpClientHandler _handler;

        public ApiService()
        {
            _handler = new HttpClientHandler
            {
                UseCookies = true,
                CookieContainer = new CookieContainer()
            };

            client = new HttpClient();
        }
        public async Task LoginAsync(string username, string password)
        {
            string loginUrl = "https://ozemmedikal.com/admin/adminlogin.php";

            // Login form verileri
            var loginData = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("username", username),
            new KeyValuePair<string, string>("pass", password),
            new KeyValuePair<string, string>("submit", "Giriş")
             });
        
            // POST isteği ile login
            var response = await client.PostAsync(loginUrl, loginData);

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Giriş işlemi başarısız oldu.");
            }

            // Başarılıysa çerezleri saklar
        }
        // HTML Tablo Çekme
        public async Task<(string TableHtml, List<int> PageNumbers)> FetchHtmlTableWithPagesAsync(int pageNumber)
        {
            string url = $"https://ozemmedikal.com/admin/siparisler.php?sayfa={pageNumber}";

            // HTML içeriğini çek
            var response = await client.GetStringAsync(url);

            // HTML'i parse et
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(response);

            // Tablonun bulunduğu form'u seç
            var formNode = htmlDoc.DocumentNode.SelectSingleNode("//form[@id='app_form']");
            if (formNode == null)
            {
                throw new Exception("Tablo içeriği bulunamadı!");
            }

            // Sayfa numaralarını seç
            var pageNodes = htmlDoc.DocumentNode.SelectNodes("//a[contains(@href, 'siparisler.php?sayfa=')]");
            var pageNumbers = new List<int>();

            if (pageNodes != null)
            {
                foreach (var pageNode in pageNodes)
                {
                    // Sayfa numaralarını metin olarak al
                    var pageText = pageNode.InnerText.Trim();
                    if (int.TryParse(pageText, out int pageNum))
                    {
                        pageNumbers.Add(pageNum);
                    }
                }
            }

            // Tablo HTML'i ve sayfa numaralarını döndür
            return (formNode.OuterHtml, pageNumbers);
        }
    }
}
