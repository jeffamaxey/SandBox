using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AktuarieAppar.Models
{
    public class AppSettings
    {
        public StatsObligationSetting tioaring { get; set; }
        public StatsObligationSetting tjugoaring { get; set; }
        public GarantiSetting fenix { get; set; }
        public GarantiSetting inca { get; set; }
        public GarantiSetting gfs { get; set; }
    }

    public class StatsObligationSetting
    {
        public string vardepapper { get; set; }
        public DateTime forfall { get; set; }
    }

    public class GarantiSetting
    {
        public double factor { get; set; }
        public double tol { get; set; }
        public double skatt { get; set; }
        /* Endast GFS använder referens*/
        public double referens { get; set; }
    }

    public class TATSettings
    {
        public DateTime YearStartDate { get; set; }
        public AssetMap AssetMap { get; set; }
    }

    public class AzureFunctionUri
    {
        public string Uri { get; set; }
    }

    public class AssetMap
    {
        public string Equities { get; set; }
        public string Alternative { get; set; }
        public string RealEstate { get; set; }
        public string Bonds { get; set; }
        public string Total { get; set; }
    }
}
