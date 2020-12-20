using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AktuarieAppar.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace AktuarieAppar.Pages.ESG
{
    public class IndexModel : PageModel
    {
        // Brown
        [BindProperty(SupportsGet = true)]
        public string jparams { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool download { get; set; }
        private string szenarios;
        private string uri;


        public IndexModel(IConfiguration config)
        {
            uri = config.GetValue<string>("Serverless:uri");
            jparams = @"{""corr"":""[[1.0, -0.6, -0.6], [-0.6, 1.0, 1.0], [-0.6, 1.0, 1.0]]"",""n"":100,""t"":12}";
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (download)
                return GetScenarios(uri,jparams).Result;
            else
                return null;
        }

        private async Task<FileContentResult> GetScenarios(string uri, string jparams)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(uri);

                var content = new StringContent(jparams, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("", content);
                szenarios = await response.Content.ReadAsStringAsync();
            }
            return File(Encoding.UTF8.GetBytes(szenarios), "application/octet-stream", "szenarios.csv");
        }
    }
}