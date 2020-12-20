using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;

namespace AktuarieAppar.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IConfiguration _config;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(IConfiguration config, ILogger<IndexModel> logger)
        {
            _config = config;
            _logger = logger;
        }
        public void OnGet()
        {
            _logger.LogInformation("AktuarieAppar main entry {YearStartDate}", _config.GetValue<DateTime>("TATParametrar:yearstartdate"));
        }
    }
}