using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using AktuarieAppar.Data;
using AktuarieAppar.Models.SGB;
using AktuarieAppar.Models;

namespace AktuarieAppar.Utils.SGB
{
    public class StatsObligationer
    {
        public StatsObligation tioaring;
        public StatsObligation tjugoaring;

        public StatsObligationer(DWAktuarieContext c, IConfiguration config)
        {
            tioaring = new StatsObligation(c.vBond, 10, config);
            tjugoaring = new StatsObligation(c.vBond, 20, config);
        }
    }

    public class StatsObligation : StatsObligationSetting
    {
        private readonly IConfiguration _config;
        DbSet<vBond> _bonds;
        public DateTime LastValue;
        private int _duration;
        private AppSettings _settings; //appsettings.json
        private IDictionary<DateTime, double> PriceSeries { get; set; }
        private IDictionary<DateTime, double> AverageSeries { get; set; }

        public StatsObligation(DbSet<vBond> bonds, int duration, IConfiguration config)
        {
            _config = config;
            _bonds = bonds;
            _duration = duration;
            _settings = _config.GetSection("StatsObligationer").Get<AppSettings>();

            int plotMonths = 4;
            int averageMonths = 3; //Moving average

            LastValue = _bonds.Select(s => s.PRICE_DATE).Max();
            PriceSeries = new Dictionary<DateTime, double>();
            AverageSeries = new Dictionary<DateTime, double>();
            List<DateTime> dates = _bonds.Where(b => b.PRICE_DATE > LastValue.AddMonths(-plotMonths)).OrderBy(b => b.PRICE_DATE).Select(b => b.PRICE_DATE).Distinct().ToList();
            foreach (var date in dates)
            {
                double par = getPrice(date);
                double avg = getAverage(date, averageMonths);

                PriceSeries.Add(date, par);
                AverageSeries.Add(date, avg);
            }
        }

        public string getName()
        {
            if (_duration == 10)
                return _settings.tioaring.vardepapper;
            else if (_duration == 20)
                return _settings.tjugoaring.vardepapper;
            else
                return "missing";
        }
        public DateTime getMaturity()
        {
            if (_duration == 10)
                return _settings.tioaring.forfall;
            else if (_duration == 20)
                return _settings.tjugoaring.forfall;
            else
                return DateTime.MinValue;
        }

        public double getPrice(DateTime date)
        {
            return (double)_bonds.Where(b => b.SEC_SHORT_NAME == getName() && b.PRICE_DATE == date).Select(b => b.PRICE).SingleOrDefault();
        }
        public double getAverage(DateTime date, int delay)
        {
            DateTime fromDate = date.AddMonths(-delay);
            return (double)_bonds.Where(b => b.SEC_SHORT_NAME == getName() && b.PRICE_DATE > fromDate).Average(b => b.PRICE);
        }

        public IDictionary<DateTime, double> getPriceSeries()
        {
            return PriceSeries;
        }

        public IDictionary<DateTime, double> getAverageSeries()
        {
            return AverageSeries;
        }
    }
}
