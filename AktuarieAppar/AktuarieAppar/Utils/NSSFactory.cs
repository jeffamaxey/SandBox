using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AktuarieAppar.Data;
using AktuarieAppar.Models.NSS;

namespace AktuarieAppar.Utils.NSS
{
    /// <summary>
    /// Beräknar diskonteringsräntor från swapar enligt FFFS 2013:23
    /// och passar därefter Nelson-Siegel-Svensson parametrar till dem. 
    /// </summary>
    public class NSSFactory
    {
        public double[] GetFiRates() { return FiRates; }
        public double[] GetFiDelta() { return FiDeltas; }
        public double[] GetDeltasNSS() { return NSSDeltas; }
        public int[] GetTimesteps() { return years; }
        public double[] GetParAdjustedTJ() { return ParAdjusted; } //Debug
        public bool OptimizationConvergedDF = false;
        public bool OptimizationConvergedNSS = false;

        private RegressionResult _resultDF;
        private RegressionResult _resultNSS;
        private double[] ParAdjusted;
        private double[] FiRates;
        private double[] FiDeltas;
        private double[] NSSDeltas;

        private int[] years;
        private readonly int n_years = 101;
        //Interpolation intervals
        private readonly int T1 = 10;
        private readonly int T2 = 20;
        //Ultimate forward rate
        private double UFR;


        public NSSFactory(IDictionary<int, double> dSwaps, double ufr, double creditRisk, double golv, NSSParametrar initGuess)
        {
            int[] swapdur = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20 };
            if (!swapdur.All(dSwaps.ContainsKey))
                throw new KeyNotFoundException();
            UFR = ufr;

            int[] dur = new int[20];  
            double[] par = new double[20];
            for (int i = 0; i < 20; i++)
            {
                dur[i] = i + 1;
                try
                {
                    par[i] = dSwaps[i + 1];
                }
                catch (KeyNotFoundException e)
                {
                    par[i] = 999.9; //dummy
                }
            }
            ParAdjusted = AdjustPar(par, creditRisk, golv);
            _resultDF = OptimizeDiscountFactors(ParAdjusted);
            if (_resultDF.TerminationReason == TerminationReason.Converged)
                OptimizationConvergedDF = true;

            // Fi diskonteringsräntekurva enl FFFS 2013:23
            FiRates = GetFiRate(n_years);
            FiDeltas = MakeDelta(FiRates);
            years = new int[n_years];
            for (int i = 0; i < n_years; i++)
                years[i] = i;

            _resultNSS = OptimizeNSSparameters(years, FiDeltas, initGuess);
            if (_resultNSS.TerminationReason == TerminationReason.Converged)
                OptimizationConvergedNSS = true;
            NSSDeltas = MakeDelta(_resultNSS.Constants, years);
        }

        public double DiscountFactor(int dur)
        {  //Reset DF to 1 where par is 0
            if (dur == 0 || ParAdjusted[dur - 1] == 0.0)
                return 1.0;
            else
                return _resultDF.Constants[dur - 1];
        }

        public double ZeroCouponRate(int dur)
        {
            return Math.Pow(DiscountFactor(dur), -1 / (double)dur) - 1.0;
        }

        public double ForwardRate(int dur)
        {
            return DiscountFactor(dur - 1) / DiscountFactor(dur) - 1.0;
        }

        public double WeightedForwardRate(int dur)
        {
            double t = (double)dur;
            if (t <= T1)
                return ForwardRate(dur);
            else if (t <= T2)
                return (T2 - t + 1) / (T2 - T1 + 1) * ForwardRate(dur) + (t - T1) / (T2 - T1 + 1) * UFR;
            else
                return UFR;
        }

        private double[] AdjustPar(double[] par, double creditrisk, double golv)
        {
            double[] parAdjusted = new double[par.Length];
            for (int i = 0; i < par.Length; i++)
                parAdjusted[i] = Math.Max(golv, par[i] / 100.0 - creditrisk);
            return parAdjusted;
        }

        private double[] GetFiRate(int maxdur)
        {
            double[] fiRate = new double[maxdur];
            fiRate[0] = ZeroCouponRate(1); // Fixa!
            fiRate[1] = ZeroCouponRate(1);
            for (int t = 2; t < maxdur; t++)
                fiRate[t] = Math.Pow(Math.Pow(1 + fiRate[t - 1], t - 1) * (1 + WeightedForwardRate(t)), 1 / (double)t) - 1;

            return fiRate;
        }

        private double[] MakeDelta(double[] rates)
        {
            double[] deltas = new double[rates.Length];
            for (int i = 0; i < rates.Length; i++)
                deltas[i] = Math.Log(1 + rates[i]);
            return deltas;
        }

        private double[] MakeDelta(double[] nss, int[] tt)
        {
            double[] deltaNSS = new double[n_years];
            for (int i = 0; i < n_years; i++)
                deltaNSS[i] = NSSFactory.Delta(nss, tt[i]);
            return deltaNSS;
        }


        /// <summary>
        /// Finds discount factors DF_t from par-rates p_t s.t.
        /// p_t sum(DF_t) = 1 - DF_t
        /// </summary>
        /// <param name="_"></param> NOT USED
        /// <param name="par"></param>
        /// <returns></returns>
        private RegressionResult OptimizeDiscountFactors(double[] par)
        {
            // Optimization
            double tolerance = 1e-6;
            int maxEvals = 10000;
            SimplexConstant[] DF0 = new SimplexConstant[]
            {   // (Initial guess, Initial perturbation)
                new SimplexConstant(1.0, 0.1), new SimplexConstant(1.0, 0.1), new SimplexConstant(1.0, 0.1),
                new SimplexConstant(1.0, 0.1), new SimplexConstant(1.0, 0.1), new SimplexConstant(1.0, 0.1),
                new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1),
                new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1),
                new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1), new SimplexConstant(0.8, 0.1),
                new SimplexConstant(0.5, 0.1), new SimplexConstant(0.5, 0.1), new SimplexConstant(0.5, 0.1),
                new SimplexConstant(0.5, 0.1), new SimplexConstant(0.5, 0.1)
            };
            ObjectiveFunctionDelegate objFunction = new ObjectiveFunctionDelegate(ObjectiveDF);
            return NelderMeadSimplex.Regress(DF0, Array.Empty<int>(), par, tolerance, maxEvals, objFunction);
        }

        private static double ObjectiveDF(double[] DF, int[] T, double[] par)
        {
            if (!DF.All(df => df > 0))
                return 100.0;
            else
            {
                double ssq =
                Math.Pow(par[0] * DF[0] + DF[0] - 1, 2) +
                Math.Pow(par[1] * DF.Slice(0, 2).Sum() + DF[1] - 1, 2) +
                Math.Pow(par[2] * DF.Slice(0, 3).Sum() + DF[2] - 1, 2) +
                Math.Pow(par[3] * DF.Slice(0, 4).Sum() + DF[3] - 1, 2) +
                Math.Pow(par[4] * DF.Slice(0, 5).Sum() + DF[4] - 1, 2) +
                Math.Pow(par[5] * DF.Slice(0, 6).Sum() + DF[5] - 1, 2) +
                Math.Pow(par[6] * DF.Slice(0, 7).Sum() + DF[6] - 1, 2) +
                Math.Pow(par[7] * DF.Slice(0, 8).Sum() + DF[7] - 1, 2) +
                Math.Pow(par[8] * DF.Slice(0, 9).Sum() + DF[8] - 1, 2) +
                Math.Pow(par[9] * DF.Slice(0, 10).Sum() + DF[9] - 1, 2) +
                    // log-linear interpolation for maturities 11, 13, 14, 16, 17, 18 and 19 years
                    Math.Pow(DF[10] - Math.Exp((Math.Log(DF[9]) + Math.Log(DF[11])) / 2), 2) +
                Math.Pow(par[11] * DF.Slice(0, 12).Sum() + DF[11] - 1, 2) +
                    Math.Pow(DF[12] - Math.Exp(Math.Log(DF[11]) * 2 / 3 + Math.Log(DF[14]) / 3), 2) +
                    Math.Pow(DF[13] - Math.Exp(Math.Log(DF[11]) / 3 + Math.Log(DF[14]) * 2 / 3), 2) +
                Math.Pow(par[14] * DF.Slice(0, 15).Sum() + DF[14] - 1, 2) +
                    Math.Pow(DF[15] - Math.Exp(Math.Log(DF[14]) * 4 / 5 + Math.Log(DF[19]) / 5), 2) +
                    Math.Pow(DF[16] - Math.Exp(Math.Log(DF[14]) * 3 / 5 + Math.Log(DF[19]) * 2 / 5), 2) +
                    Math.Pow(DF[17] - Math.Exp(Math.Log(DF[14]) * 2 / 5 + Math.Log(DF[19]) * 3 / 5), 2) +
                    Math.Pow(DF[18] - Math.Exp(Math.Log(DF[14]) / 5 + Math.Log(DF[19]) * 4 / 5), 2) +
                Math.Pow(par[19] * DF.Slice(0, 20).Sum() + DF[19] - 1, 2);
                return ssq;
            }
        }

        public double MeanSquaredErrorDF()
        {   // years as dummy
            return ObjectiveDF(_resultDF.Constants, years, ParAdjusted) / _resultDF.Constants.Length; ;
        }



        /// <summary>
        /// Given yield-curve y
        /// Find Nelson-Siegel-Svensson parameters
        /// </summary>
        /// <param name="t"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private RegressionResult OptimizeNSSparameters(int[] t, double[] y, NSSParametrar nss)
        {
            double tolerance = 1e-10;
            int maxEvals = 10000;
      
            SimplexConstant[] nss_init = new SimplexConstant[] { new SimplexConstant(nss.Betha0, 0.1),
                new SimplexConstant(nss.Betha1, 0.1), new SimplexConstant(nss.Betha2, 0.1), new SimplexConstant(nss.Betha3, 0.1),
                new SimplexConstant(nss.Tao0, 0.1), new SimplexConstant(nss.Tao1, 0.1) };
            ObjectiveFunctionDelegate objFunction = new ObjectiveFunctionDelegate(ObjectiveNSS);

            return NelderMeadSimplex.Regress(nss_init, t, y, tolerance, maxEvals, objFunction);
        }

        public static double ObjectiveNSS(double[] x, int[] T, double[] Y)
        {
            // tau0, tau1 > 0
            if (!(x[4] > 0.0 && x[5] > 0.0))
                return 1.0;
            else
            {
                double ssq = 0;
                for (int i = 0; i < T.Length; i++)
                {
                    double yRegress = Delta(x, T[i]);
                    ssq += Math.Pow((Y[i] - yRegress), 2);
                }
                return ssq;
            }
        }

        /// <summary>
        /// Nelson-Siegel-Svensson parametrization of yieldcurve
        /// </summary>
        /// <param name="nss"></param>
        /// <param name="t"></param>
        /// <returns>rate as intensity</returns>
        public static double Delta(double[] nss, double t)
        {
            if (t < Double.Epsilon)
                return nss[0] + nss[1];
            else
                return nss[0] + nss[1] * (1 - Math.Exp(-t / nss[4])) / (t / nss[4])
                            + nss[2] * ((1 - Math.Exp(-t / nss[4])) / (t / nss[4]) - Math.Exp(-t / nss[4]))
                            + nss[3] * ((1 - Math.Exp(-t / nss[5])) / (t / nss[5]) - Math.Exp(-t / nss[5]));
        }

        public NSSParametrar GetNSSParametrar()
        {
            NSSParametrar nss = new NSSParametrar();
            nss.Betha0 = Math.Round(_resultNSS.Constants[0], 9);
            nss.Betha1 = Math.Round(_resultNSS.Constants[1], 9);
            nss.Betha2 = Math.Round(_resultNSS.Constants[2], 9);
            nss.Betha3 = Math.Round(_resultNSS.Constants[3], 9);
            nss.Tao0 = Math.Round(_resultNSS.Constants[4], 9);
            nss.Tao1 = Math.Round(_resultNSS.Constants[5], 9);
            return nss;
        }

        public double MeanSquaredErrorNSS()
        {
            return ObjectiveNSS(_resultNSS.Constants, years, FiDeltas) / n_years; ;
        }
    }


    public class Swap
    {
        public List<DateTime> ValueDates;
        public DateTime LastValue;

        private DbSet<vSwap> _vSwap;
        private int PriceType = 28; //Traded_EOD

        public Swap(DWMartarContext c)
        {

            _vSwap = c.vSwap;
            ValueDates = c.vSwap.Where(s => s.PRICETYPE == PriceType).Select(s => s.PRICEDATE).Distinct().OrderByDescending(s => s).ToList();
            LastValue = c.vSwap.Where(s => s.PRICETYPE == PriceType).Select(s => s.PRICEDATE).Max();
        }
        public async Task<Dictionary<int, double>> GetSwapQuotesAsync(DateTime date)
        {
            //ToDictionaryAsync(e => e.Maturity, e => e.QUOTE);
            var q = _vSwap.OrderBy(s => s.Maturity).
                Where(s => s.PRICEDATE == date && s.PRICETYPE == PriceType && s.Maturity > 0).
                Select(s => new { s.Maturity, s.QUOTE });
            Dictionary<int, double> res = new Dictionary<int, double>();
            foreach (var item in q)
                res.Add(item.Maturity, (double)item.QUOTE);

            return res;
        }

        public async Task<DateTime> GetPreviousSwapDateAsync(DateTime date)
        {
            int Year = date.AddMonths(-1).Year;
            int prevMonth = date.AddMonths(-1).Month;
            DateTime prev = await _vSwap.Where(m => m.PRICEDATE.Year == Year && m.PRICEDATE.Month == prevMonth)
                .MaxAsync(m => m.PRICEDATE);

            return prev;
        }
    }
}

public static class Extensions
{
    // Pythonic syntax
    public static T[] Slice<T>(this T[] data, int start, int end)
    {   
        int len = end - start;
        T[] result = new T[len];
        Array.Copy(data, start, result, 0, len);
        return result;
    }
}
