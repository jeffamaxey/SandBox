using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using AktuarieAppar.Data;
using AktuarieAppar.Utils;
using AktuarieAppar.Utils.NSS;
using AktuarieAppar.Security;
using AktuarieAppar.Models.NSS;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace AktuarieAppar.Pages.NSS
{
    //[AuthorizeActuaryNET]
    public class IndexModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public bool Simulation { get; set; } // grunder or simulera
        [BindProperty(SupportsGet = true)]
        public DateTime ValueDate { get; set; }
        public SelectList ValueDates;
        [BindProperty(SupportsGet = true)]
        public double ufr { get; set; }
        [BindProperty(SupportsGet = true)]
        public double golv { get; set; }
        [BindProperty(SupportsGet = true)]
        public double kreditriskTJ { get; set; }
        [BindProperty(SupportsGet = true)]
        public double kreditriskPR { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime lastDateOfMonth { get; set; }
        public IDictionary<int, double> SwapQuotes { get; set; }
        public IDictionary<int, double> SwapQuotesPrevious { get; set; }
        public double[] FiRates { get; set; }
        public double[] FiRatesPrevious { get; set; }
        [BindProperty(SupportsGet = true)]
        public NSSParametrar NssParametrarInitTJ { get; set; }
        [BindProperty(SupportsGet = true)]
        public NSSParametrar NssParametrarInitPR { get; set; }
        [BindProperty]
        public NSSParametrar NssParametrarTJ { get; set; }
        [BindProperty]
        public NSSParametrar NssParametrarPR { get; set; }
        public int[] timesteps;
        public double[] deltasTJ;
        public double[] deltasPR;
        public double[] deltasNSSTJ;
        public double[] deltasNSSPR;
        public double MeanSquareErrorNSSTJ;
        public double MeanSquareErrorNSSPR;
        public double MeanSquareErrorDFTJ;
        public double MeanSquareErrorDFPR;

        private readonly Swap Swaps;
        private readonly GrunderFactory LabanGrunder;
        private NSSFactory NSSFactoryTJ;
        private NSSFactory NSSFactoryPR;
        private NSSFactory NSSFactoryTJPrevious;

        private double warningMSE = 1e-8;
        private double errorMSE = 1e-3;
        private readonly ILogger<IndexModel> _logger;

        // Debug to be deleted
        public int[] debugDur;
        public double[] debugPar;
        public double[] debugDF;
        public double[] debugZC;
        public double[] debugForward;
        public double[] debugWeightedForward;

        public IndexModel(DWMartarContext dwc, Laban01Context lc1, LabanContext lc, ILogger<IndexModel> logger)
        {
            _logger = logger;
            Swaps = new Swap(dwc);
            LabanGrunder = new GrunderFactory(lc1, lc);
            ValueDates = new SelectList(Swaps.ValueDates);
            if (ufr == 0)
            {
                ufr = 0.042;
                kreditriskTJ = 0.0035;
                kreditriskPR = 0.0055;
                golv = 0.0;
            }
        }

        public async Task OnGetAsync(string purpose)
        {
            Simulation = purpose == "simulering";
            ValueDate = ValueDate == DateTime.MinValue ? Swaps.LastValue : ValueDate;
            DateTime previousSwapDate = await Swaps.GetPreviousSwapDateAsync(ValueDate);
            DateTime previousNSSDate = LabanGrunder.GetPreviousNSSDate(ValueDate);
            SwapQuotesPrevious = await Swaps.GetSwapQuotesAsync(previousSwapDate);
            SwapQuotes = await Swaps.GetSwapQuotesAsync(ValueDate);

            // Initial guess to optimization
            if (NssParametrarInitTJ.Grunder != "InitialGuess")
            {
                NssParametrarInitTJ = LabanGrunder.GetNSSParametrar("PRES", "TJP1P", previousNSSDate);
                NssParametrarInitPR = LabanGrunder.GetNSSParametrar("PRES", "PRIV1P", previousNSSDate);
            }
            NSSFactoryTJ = new NSSFactory(SwapQuotes, ufr, kreditriskTJ, golv, NssParametrarInitTJ);
            NSSFactoryPR = new NSSFactory(SwapQuotes, ufr, kreditriskPR, golv, NssParametrarInitPR);
            NSSFactoryTJPrevious = new NSSFactory(SwapQuotesPrevious, ufr, kreditriskTJ, golv, NssParametrarInitTJ);
            if (!(NSSFactoryTJ.OptimizationConvergedDF && NSSFactoryTJ.OptimizationConvergedNSS &&
                NSSFactoryPR.OptimizationConvergedDF && NSSFactoryPR.OptimizationConvergedNSS &&
                NSSFactoryTJPrevious.OptimizationConvergedDF && NSSFactoryTJPrevious.OptimizationConvergedNSS))
                _logger.LogError("Optimization Convergence");

            FiRates = NSSFactoryTJ.GetFiRates();
            FiRatesPrevious = NSSFactoryTJPrevious.GetFiRates();

            // FromDate used in Laban INSERT (POST)
            lastDateOfMonth = new DateTime(ValueDate.Year, ValueDate.Month, DateTime.DaysInMonth(ValueDate.Year, ValueDate.Month));

            deltasTJ = NSSFactoryTJ.GetFiDelta();
            deltasPR = NSSFactoryPR.GetFiDelta();
            timesteps = NSSFactoryTJ.GetTimesteps();

            NssParametrarTJ = NSSFactoryTJ.GetNSSParametrar();
            NssParametrarPR = NSSFactoryPR.GetNSSParametrar();
            _logger.LogInformation("Nelson-Siegel-Svensson {ValueDate} {@ParRates} {@NelsonSiegelTJ}", ValueDate, SwapQuotes, NssParametrarTJ);

            deltasNSSTJ = NSSFactoryTJ.GetDeltasNSS();
            deltasNSSPR = NSSFactoryPR.GetDeltasNSS();
            MeanSquareErrorNSSTJ = NSSFactoryTJ.MeanSquaredErrorNSS();
            MeanSquareErrorDFTJ = NSSFactoryTJ.MeanSquaredErrorDF();
            MeanSquareErrorNSSPR = NSSFactoryPR.MeanSquaredErrorNSS();
            MeanSquareErrorDFPR = NSSFactoryPR.MeanSquaredErrorDF();

            if (MeanSquareErrorNSSTJ > warningMSE || MeanSquareErrorNSSPR > warningMSE || MeanSquareErrorDFTJ > warningMSE || MeanSquareErrorDFPR > warningMSE)
                _logger.LogWarning("Optimization accuracy {MeanSquareErrorNSSTJ} {MeanSquareErrorNSSPR} {MeanSquareErrorDFTJ} {MeanSquareErrorDFPR}", MeanSquareErrorNSSTJ, MeanSquareErrorNSSPR, MeanSquareErrorDFTJ, MeanSquareErrorDFPR);
            else if (MeanSquareErrorNSSTJ > errorMSE || MeanSquareErrorNSSPR > errorMSE || MeanSquareErrorDFTJ > errorMSE || MeanSquareErrorDFPR > errorMSE)
                _logger.LogError("Optimization accuracy {MeanSquareErrorNSSTJ} {MeanSquareErrorNSSPR} {MeanSquareErrorDFTJ} {MeanSquareErrorDFPR}", MeanSquareErrorNSSTJ, MeanSquareErrorNSSPR, MeanSquareErrorDFTJ, MeanSquareErrorDFPR);

            MakeDebugDisplay();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            int rows = await LabanGrunder.AddNSSParametrarAsync(NssParametrarTJ, NssParametrarPR);
            _logger.LogInformation("AddNSSParametrar {rows}", rows);
            return RedirectToPage("./Laban");
        }


     
        public double Residual(int i, string type)
        {
            if (type != "tj")
                return deltasNSSPR[i] - deltasPR[i];
            else
                return deltasNSSTJ[i] - deltasTJ[i];
        }

        public double FiRateChange(int i)
        {
            return FiRates[i] - FiRatesPrevious[i];
        }

        void MakeDebugDisplay() // Delete when done
        {
            debugDur = new int[20];
            debugDF = new double[20];
            debugZC = new double[20];
            debugForward = new double[20];
            debugWeightedForward = new double[20];
            for (int i = 0; i < 20; i++)
            {
                debugDur[i] = i + 1;
                debugPar = NSSFactoryTJ.GetParAdjustedTJ();
                debugDF[i] = NSSFactoryTJ.DiscountFactor(i + 1);
                debugZC[i] = NSSFactoryTJ.ZeroCouponRate(i + 1);
                debugForward[i] = NSSFactoryTJ.ForwardRate(i + 1);
                debugWeightedForward[i] = NSSFactoryTJ.WeightedForwardRate(i + 1);
            }
        }
    }
}
