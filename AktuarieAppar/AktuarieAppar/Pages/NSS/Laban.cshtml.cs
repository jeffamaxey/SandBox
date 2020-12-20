using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AktuarieAppar.Models.NSS;
using AktuarieAppar.Data;
using Microsoft.EntityFrameworkCore;
using AktuarieAppar.Security;

namespace AktuarieAppar.Pages.NSS
{
    [AuthorizeActuaryNET]
    public class LabanModel : PageModel
    {
        private readonly Laban01Context Laban1;
        private readonly LabanContext Laban;

        public LabanModel(Laban01Context lc1, LabanContext lc)
        {
            Laban1 = lc1;
            Laban = lc;
        }

        public IList<NSSParametrar> NssParametrar01 { get; set; }
        public IList<NSSParametrar> NssParametrar { get; set; }

        public async Task OnGetAsync()
        {
            int showMonths = 1;

            NssParametrar01 = await Laban1.NSSParametrar.Where(g => g.FromDate > DateTime.Today.AddMonths(-showMonths)).OrderByDescending(g => g.FromDate).ThenByDescending(g => g.KurvNamn).ToListAsync();
            NssParametrar = await Laban.NSSParametrar.Where(g => g.FromDate > DateTime.Today.AddMonths(-showMonths)).OrderByDescending(g => g.FromDate).ThenByDescending(g => g.KurvNamn).ToListAsync();
        }
    }
}