using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AktuarieAppar.Data;
using AktuarieAppar.Models.TAT;
using Microsoft.Extensions.Configuration;

namespace AktuarieAppar.Utils.TAT
{
    public class TATFactory
    {
        private DbSet<Avkastning> TAT;
        private DbSet<PeriodAvkastning> tAvkastning;
        private DWAktuarieContext DWAktuarie;

        public TATFactory(DWMartarContext dWMartarContext, DWAktuarieContext context, IConfiguration config)
        {       
            TAT = dWMartarContext.vTotalAvkastning;
            tAvkastning = context.tPeriodAvkastning;
            DWAktuarie = context;
        }
   
        public async Task<List<DateTime>> GetTATDatesAsync()
        {
            List<DateTime> dates = await TAT.Select(m => m.ToDate).Distinct().OrderByDescending(m => m).ToListAsync();

            return dates;
        }
        
        public async Task<DateTime> GetLatestTATDateAsync()
        {
            DateTime latest = await TAT.Select(m => m.ToDate).MaxAsync();
            return latest;
        }

        private async Task<DateTime> GetPreviousMonthEndAsync(DateTime TATDate)
        {
            int prevYear = TATDate.AddMonths(-1).Year;
            int prevMonth = TATDate.AddMonths(-1).Month;
            DateTime prev = await TAT.Where(m => m.ToDate.Year == prevYear && m.ToDate.Month == prevMonth)
                .Select(m => m.ToDate).MaxAsync();

            return prev;
        }

        // Aktuarie fördelar avkastning till systemen
        // loggas genom INSERT i AktuarieAppar
        public bool IsAllocated(DateTime TATDate)
        {
            IQueryable<PeriodAvkastning> allocated = tAvkastning.Where(d => d.ToDate == TATDate);

            return allocated.Any();
        }

        public async Task<IList<Avkastning>> GetTATAsync(DateTime TATDate)
        {
            List<Avkastning> tat = await TAT.Where(t => t.ToDate == TATDate)
                .OrderBy(t => t.NodeName).ToListAsync();
            return tat;
        }

        public async Task<IList<TotalAvkastning>> GetTotalTATAsync(DateTime TATDate)
        {
            List<TotalAvkastning> tot = await TAT.Where(t => t.ToDate >= TATDate.AddYears(-1) && t.ToDate <= TATDate)
                .GroupBy(t => new { t.ToDate, t.ValidDate })
                .Select(s => new TotalAvkastning
                {
                    ToDate = s.Key.ToDate,
                    NodeName = "Totalt",
                    MarketValueUB = s.Sum(x => x.MarketValueUB),
                    ReturnSekYtd = s.Sum(x => x.ReturnSekYtd),
                    ValidDate = s.Key.ValidDate
                })
                .OrderByDescending(t => t.ToDate)
                .ToListAsync();
            return tot;
        }

        public async Task<IList<PeriodAvkastning>> GetPeriodReturnAsync(DateTime TATDate)
        {
            DateTime TATDatePrevious = await GetPreviousMonthEndAsync(TATDate);
            List<Avkastning> tat = await TAT.Where(t => t.ToDate == TATDate).ToListAsync();
            List<PeriodAvkastning> avk = new List<PeriodAvkastning>();
            foreach (Avkastning row in tat)
            {
                decimal previousreturn = await TAT.Where(t => t.ToDate == TATDatePrevious && t.NodeName == row.NodeName)
                    .Select(t => t.ReturnSekYtd).FirstOrDefaultAsync();
                PeriodAvkastning per = new PeriodAvkastning
                {
                    ToDate = row.ToDate,
                    NodeName = row.NodeName,
                    ReturnSekPeriod = row.ReturnSekYtd - previousreturn,
                    InsertDate = DateTime.Now
                };
                avk.Add(per);
            }
            return avk;
        }

        public async Task<int> SavePeriodReturnAsync(IList<PeriodAvkastning> PeriodAvkastningar)
        {
            tAvkastning.AddRange(PeriodAvkastningar);
            return await DWAktuarie.SaveChangesAsync();
        }
    }
}

