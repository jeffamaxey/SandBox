using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AktuarieAppar.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using AktuarieAppar.Security;
using AktuarieAppar.Utils.TAT;
using AktuarieAppar.Models.TAT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AktuarieAppar.Pages.TAT
{
    [AuthorizeActuaryNET]
    public class IndexModel : PageModel
    {
        public SelectList TATDates;
        [BindProperty(SupportsGet = true)]
        public DateTime TATDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public bool AvkastningUtdelad { get; set; }
        [BindProperty]
        public IList<Avkastning> Avkastningar { get; set; }
        [BindProperty]
        public IList<PeriodAvkastning> PeriodAvkastningar { get; set; }
        [BindProperty]
        public IList<TotalAvkastning> TotalAvkastningar { get; set; }
        public Dictionary<string, string> AssetNameMap;

        private readonly TATFactory TATFactory;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(DWAktuarieContext dw, DWMartarContext dwmart, IConfiguration config, ILogger<IndexModel> logger)
        {
            TATFactory = new TATFactory(dwmart, dw, config);
            AssetNameMap = new Dictionary<string, string> {
                { "AK", "Aktier" },
                { "Alternativa", "Alternativa" },
                { "Fast", "Fastigheter" },
                { "RB", "Obligationer" },
                { "Totalt", "Totalt" }
            };
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            DateTime LatestTAT = await TATFactory.GetLatestTATDateAsync();
            TATDate = TATDate == DateTime.MinValue || TATDate > LatestTAT ? LatestTAT : TATDate;
            List<DateTime> TATDATEList = await TATFactory.GetTATDatesAsync();
            TATDates = new SelectList(TATDATEList.OrderByDescending(x => x));
            AvkastningUtdelad = TATFactory.IsAllocated(TATDate);

            try
            {
                Avkastningar = await TATFactory.GetTATAsync(TATDate);
                PeriodAvkastningar = await TATFactory.GetPeriodReturnAsync(TATDate);
                TotalAvkastningar = await TATFactory.GetTotalTATAsync(TATDate);
                _logger.LogInformation("GET {@TAT} {@PeriodTAT}", Avkastningar, PeriodAvkastningar);
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError("GET {@TAT} {@Exception}", Avkastningar, e);
            }
        }
    
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            string insertmessage;
            try
            {
                int rows = await TATFactory.SavePeriodReturnAsync(PeriodAvkastningar);
                _logger.LogInformation("POST {@PeriodTAT}", PeriodAvkastningar);
                insertmessage = rows + " rows inserted!";
            }
            catch (Microsoft.EntityFrameworkCore.DbUpdateException e)
            {
                insertmessage = e.InnerException.Message;
            }
            catch (Exception e)
            {
                insertmessage = e.Message;
            }
            return RedirectToPage("./PeriodAvkastning", new { message = insertmessage });
        }
    }
}
