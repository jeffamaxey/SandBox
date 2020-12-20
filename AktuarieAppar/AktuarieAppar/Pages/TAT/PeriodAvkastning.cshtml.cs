using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using AktuarieAppar.Data;
using AktuarieAppar.Security;
using AktuarieAppar.Models.TAT;

namespace AktuarieAppar.Pages.TAT
{
    [AuthorizeActuary]
    public class PeriodAvkastningModel : PageModel
    {
        private readonly DWAktuarieContext _context;

        public PeriodAvkastningModel(DWAktuarieContext context)
        {
            _context = context;
        }

        public string InsertMessage;
        public IList<PeriodAvkastning> Avkastningar { get; set; }

        public async Task OnGetAsync(string message)
        {
            InsertMessage = message;
            Avkastningar = await _context.tPeriodAvkastning.OrderBy(m => m.NodeName)
                .OrderByDescending(m => m.InsertDate)
                .ToListAsync();
        }
    }
}