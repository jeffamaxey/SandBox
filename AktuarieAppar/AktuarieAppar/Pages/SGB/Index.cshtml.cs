using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AktuarieAppar.Utils;
using AktuarieAppar.Data;
using AktuarieAppar.Utils.SGB;
using Microsoft.Extensions.Configuration;
using AktuarieAppar.Security;
using AktuarieAppar.Models;
using Microsoft.Extensions.Logging;

namespace AktuarieAppar.Pages.SGB
{
    [AuthorizeActuaryNET]
    public class IndexModel : PageModel
    {
        private readonly StatsObligationer _statsobligationer;
        private readonly GrunderFactory grunder;

        public IDictionary<DateTime, double> PriceSeries10 { get; set; }
        public IDictionary<DateTime, double> PriceSeries20 { get; set; }
        public IDictionary<DateTime, double> AverageSeries10 { get; set; }
        public IDictionary<DateTime, double> AverageSeries20 { get; set; }
        public IDictionary<DateTime, double[]> PriceSeries { get; set; }
        public IDictionary<DateTime, double[]> DiffSeries { get; set; }
        public DateTime ValueDate { get; set; }
        public string vardepapper10 { get; set; }
        public DateTime forfall10 { get; set; }
        public string vardepapper20 { get; set; }
        public DateTime forfall20 { get; set; }


        public double SGB20Y_3m { get; set; }
        public double SGB10Y_3m { get; set; }
        public double SGB20Y { get; set; }
        public double SGB10Y { get; set; }
        public double premiegrundFenix { get; set; }
        public double premiegrundInca { get; set; }
        public double premiegrundGFS { get; set; }
        public double referensGFS { get; set; }
        public double factorFenix { get; set; }
        public double factorInca { get; set; }
        public double factorGFS { get; set; }
        private double skattFenix { get; set; }
        private double skattInca { get; set; }
        private double skattGFS { get; set; }
        public double tol;
        public bool uppdateraFenix = false;
        public bool uppdateraInca = false;
        public bool uppdateraGFS = false;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(DWAktuarieContext c, Laban01Context lc, IConfiguration config, ILogger<IndexModel> logger)
        {
            _logger = logger;
            _statsobligationer = new StatsObligationer(c, config);
            var settings = config.GetSection("GarantiParametrar").Get<AppSettings>();
            grunder = new GrunderFactory(lc);
            tol = settings.inca.tol;
            factorFenix = settings.fenix.factor;
            factorInca = settings.inca.factor;
            factorGFS = settings.gfs.factor;
            skattFenix = settings.fenix.skatt;
            skattInca = settings.inca.skatt;
            skattGFS = settings.gfs.skatt;
            referensGFS = settings.gfs.referens;
        }

        public async Task OnGetAsync()
        {
            ValueDate = _statsobligationer.tioaring.LastValue;
            vardepapper10 = _statsobligationer.tioaring.getName();
            forfall10 = _statsobligationer.tioaring.getMaturity();
            vardepapper20 = _statsobligationer.tjugoaring.getName();
            forfall20 = _statsobligationer.tjugoaring.getMaturity();
            PriceSeries10 = _statsobligationer.tioaring.getPriceSeries();
            PriceSeries20 = _statsobligationer.tjugoaring.getPriceSeries();
            AverageSeries10 = _statsobligationer.tioaring.getAverageSeries();
            AverageSeries20 = _statsobligationer.tjugoaring.getAverageSeries();
            _logger.LogInformation("Garantiränta {ValueDate} {vardepapper10} {forfall10} {vardepapper20} {forfall20}",
                                   ValueDate, vardepapper10, forfall10, vardepapper20, forfall20);
            SGB20Y_3m = _statsobligationer.tjugoaring.getAverage(ValueDate, 3);
            SGB10Y_3m = _statsobligationer.tioaring.getAverage(ValueDate, 3);
            SGB20Y = _statsobligationer.tjugoaring.getPrice(ValueDate);
            SGB10Y = _statsobligationer.tioaring.getPrice(ValueDate);

            premiegrundFenix = Math.Round(100 * grunder.GetPremieGrund("fenix", skattFenix), 2);
            premiegrundInca = Math.Round(100*grunder.GetPremieGrund("inca", skattInca), 2);
            premiegrundGFS = Math.Round(100 * grunder.GetGarantiForstarkning(skattGFS), 2);

            uppdateraFenix = withinLimit(SGB10Y, SGB10Y_3m, factorFenix, premiegrundFenix, tol);
            uppdateraInca = withinLimit(SGB20Y, SGB20Y_3m, factorInca, premiegrundInca, tol);
            uppdateraGFS = withinLimit(SGB20Y, SGB20Y_3m, factorGFS, referensGFS, tol);

            DiffSeries = getDiffSeries(PriceSeries10, PriceSeries20, AverageSeries10, AverageSeries20,
                                       premiegrundFenix, premiegrundInca, referensGFS,
                                       factorFenix, factorInca, factorGFS,
                                       tol);

            bool withinLimit(double bond, double bond_3m, double factor, double r, double tol)
            {
                if (Math.Abs(bond * factor - r) > tol && Math.Abs(bond_3m * factor - r) > tol)
                    return true;
                else
                    return false;
            }
        }

        private static Dictionary<DateTime, double[]> getDiffSeries(IDictionary<DateTime, double> series10, IDictionary<DateTime, double> series20,
                                                         IDictionary<DateTime, double> average10, IDictionary<DateTime, double> average20,
                                                         double currentFenix, double currentInca, double currentGFS,
                                                         double factorFenix, double factorInca, double factorGFS,
                                                         double tolerance)
        {
            Dictionary<DateTime, double[]> d = new Dictionary<DateTime, double[]>();
            foreach (DateTime date in series10.Keys)
            {
                double min10 = series10[date] < average10[date] ? series10[date] : average10[date];
                double max10 = series10[date] > average10[date] ? series10[date] : average10[date];
                double fenix1 = currentFenix - min10 * factorFenix;
                double fenix2 = currentFenix - max10 * factorFenix;

                double min20 = series20[date] < average20[date] ? series20[date] : average20[date];
                double max20 = series20[date] > average20[date] ? series20[date] : average20[date];
                double inca1 = currentInca - min20 * factorInca;
                double inca2 = currentInca - max20 * factorInca;

                double gfs1 = currentGFS - min20 * factorGFS;
                double gfs2 = currentGFS - max20 * factorGFS;

                d.Add(date, new double[] { fenix1, fenix2, inca1, inca2, gfs1, gfs2, -tolerance, tolerance });
            }
            return d;
        }
    }
}
