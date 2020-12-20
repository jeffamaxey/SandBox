using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AktuarieAppar.Data;
using AktuarieAppar.Models.NSS;

namespace AktuarieAppar.Utils
{
    public class GrunderFactory
    {
        public List<DateTime> ValueDates;
        public DateTime LastValue;

        private readonly Laban01Context Laban01;
        private readonly LabanContext Laban;

        public GrunderFactory(Laban01Context lc1, LabanContext lc)
        {
            Laban01 = lc1;
            Laban = lc;
        }

        public GrunderFactory(Laban01Context lc1)
        {
            Laban01 = lc1;
        }

        public double GetPremieGrund(string koll, double skatt)
        {
            string gren;
            if (koll == "fenix")
                gren = "NS";
            else
                gren = "SL";

            DateTime from = Laban01.Grunder.Where(g => g.Miljo == "P" && g.Bolag == "TRAD" && g.Gren == gren && g.Grunder == "PGR").Select(g => g.FromDate).Max();
            double delta = Laban01.Grunder.Where(g => g.Miljo == "P" && g.Bolag == "TRAD" && g.Gren == gren && g.Grunder == "PGR" && g.FromDate == from)
                           .Select(g => g.Delta).First();

            return (Math.Exp(delta) - 1)/(1-skatt);
        }

        public double GetGarantiForstarkning(double skatt)
        {
            string miljo = "P";
            string gren = "SL";

            DateTime from = Laban01.Grunder.Where(g => g.Miljo == miljo && g.Bolag == "TRAD" && g.Gren == gren && g.Grunder == "GFS").Select(g => g.FromDate).Max();
            double delta = Laban01.Grunder.Where(g => g.Miljo == miljo && g.Bolag == "TRAD" && g.Gren == gren && g.Grunder == "GFS" && g.FromDate == from)
                           .Select(g => g.Delta).First();

            return (Math.Exp(delta) - 1) + skatt;
        }

        public DateTime GetPreviousNSSDate(DateTime date)
        {
            int previousMonth = date.Month - 1;
            int year = date.Year;
            if (previousMonth == 0)
            {
                previousMonth = 12;
                year -= 1;
            }
            return Laban01.NSSParametrar.Where(g => g.FromDate.Year == year && g.FromDate.Month == previousMonth)
                .Max(g => g.FromDate);
        }

        public NSSParametrar GetNSSParametrar(string grunder, string type, DateTime fromDate)
        {
            try
            {
                return Laban01.NSSParametrar.Where(m => m.FromDate == fromDate && m.Grunder == grunder && m.KurvNamn == type)
                .First();
            }
            catch
            {
                return new NSSParametrar { Betha0 = 0.4, Betha1 = 0, Betha2 = 0, Betha3 = 0, Tao0 = 1, Tao1 = 5 };
            }
        }

        public async Task<int> AddNSSParametrarAsync(NSSParametrar nssTJ, NSSParametrar nssPR)
        {
            int rows = 0;
            nssPR.FromDate = nssTJ.FromDate;

            if (Laban.NSSParametrar.Find(nssTJ.Grunder, nssTJ.KurvNamn, nssTJ.FromDate) == null ||
                Laban.NSSParametrar.Find(nssPR.Grunder, nssPR.KurvNamn, nssPR.FromDate) == null)
            {
                // Aktuarie
                Laban.NSSParametrar.Add(nssTJ);
                Laban.NSSParametrar.Add(nssPR);
                rows = await Laban.SaveChangesAsync();
                nssTJ.Grunder = "OMFG";
                nssTJ.KurvNamn = "OMF1";
                Laban.NSSParametrar.Add(nssTJ);
                rows += await Laban.SaveChangesAsync();

                // Aktuarie01
                nssTJ.Grunder = "PRES";
                nssTJ.KurvNamn = "TJP1P";
                Laban01.NSSParametrar.Add(nssTJ);
                nssPR.KurvNamn = "PRIV1P";
                Laban01.NSSParametrar.Add(nssPR);
                rows += await Laban01.SaveChangesAsync();
                nssTJ.Grunder = "OMFG";
                nssTJ.KurvNamn = "OMF1P";
                Laban01.NSSParametrar.Add(nssTJ);
                rows += await Laban01.SaveChangesAsync();
            }
            return rows;
        }

        public IList<NSSParametrar> getNSS()
        {
            return Laban01.NSSParametrar.Where(g => g.FromDate > DateTime.Now.AddMonths(-3)).OrderByDescending(g => g.FromDate).ThenByDescending(g => g.KurvNamn).ToList();
        }
    }
}
