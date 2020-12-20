using NUnit.Framework;
using AktuarieAppar.Utils.NSS;
using AktuarieAppar.Models.NSS;
using System.Collections.Generic;
using System;

namespace AktuarieAppar.Tests
{
    [TestFixture]
    public class Tests
    {
        private IDictionary<int, double> SwapQuotes;
        private NSSParametrar NssParametrarInit;
        private NSSFactory NSSFactory;
        double[] FiFFFS;

        private double MaxError(double[] u, double[] v)
        {
            double maxerror = 0.0;
            for (int i = 0; i < u.Length; i++)
                maxerror = (Math.Abs(v[i] - u[i]) > maxerror) ? Math.Abs(v[i] - u[i]) : maxerror;
            return maxerror;
        }


        [SetUp]
        public void Setup()
        {
            // Fi par 2019-12-31
            int[] swapdur = new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 15, 20, 30 };
            double[] par_debug = new double[] { 0.1, 0.188, 0.215, 0.255, 0.318, 0.375, 0.445, 0.51, 0.57, 0.625, 0.677, 0.773, 0.873, 0.958, 1.0 }; // Fi December
            SwapQuotes = new Dictionary<int, double>();
            for (int i = 0; i < swapdur.Length; i++)
                SwapQuotes.Add(swapdur[i], par_debug[i]);

            NssParametrarInit = new NSSParametrar { Betha0 = 0.4, Betha1 = 0, Betha2 = 0, Betha3 = 0, Tao0 = 1, Tao1 = 5 };
            NSSFactory = new NSSFactory(SwapQuotes, 0.042, 0.0035, 0.0, NssParametrarInit);

            // Fi rate 2019-12-31
            FiFFFS = new double[] { 0, 0, 0, 0, 0, 0.000250125, 0.000952065, 0.001606129, 0.002212058, 0.002769566, 0.003298784, 0.004104149, 0.005022016,
                0.006039549, 0.007121224, 0.008254043, 0.009411152, 0.010607462, 0.011836308, 0.013092432, 0.014371629, 0.015670499, 0.016852732, 0.017933365,
                0.018924953, 0.019838068, 0.020681669, 0.021463404, 0.022189836, 0.022866634, 0.023498716, 0.024090373, 0.024645361, 0.025166988, 0.025658174,
                0.026121507, 0.026559292, 0.026973585, 0.027366227, 0.027738872, 0.02809301, 0.028429987, 0.028751019, 0.029057213, 0.029349575, 0.02962902,
                0.029896386, 0.03015244, 0.030397885, 0.030633367, 0.03085948, 0.031076772, 0.03128575, 0.031486883, 0.031680603, 0.031867313, 0.032047387,
                0.032221172, 0.032388993, 0.03255115, 0.032707927, 0.032859586, 0.033006374, 0.033148522, 0.033286246, 0.03341975, 0.033549226, 0.033674851,
                0.033796797, 0.033915221, 0.034030275, 0.034142101, 0.034250831, 0.034356594, 0.034459509, 0.034559689, 0.034657242, 0.03475227, 0.03484487,
                0.034935133, 0.035023148, 0.035108996, 0.035192758, 0.035274508, 0.035354317, 0.035432255, 0.035508385, 0.035582771, 0.035655472, 0.035726544,
                0.035796041, 0.035864015, 0.035930516, 0.035995591, 0.036059285, 0.036121642, 0.036182704, 0.03624251, 0.036301099, 0.036358508, 0.036414771 };
        }

        [Test]
        public void TestMaxerrorFiFFFSisLessThanTolerance()
        {
            double tol = 1.0E-8;
            double[] FiRates = NSSFactory.GetFiRates();
            double maxerror = MaxError(FiFFFS, FiRates);
            Assert.LessOrEqual(maxerror, tol);
        }

        [Test]
        public void TestMaxerrorNSSisLessThanTolerance()
        {
            double tol = 1.0E-3;
            double[] deltas = NSSFactory.GetFiDelta();
            double[] deltasNSS = NSSFactory.GetDeltasNSS();
            double maxerror = MaxError(deltas, deltasNSS);
            Assert.LessOrEqual(maxerror, tol);
        }
    }
}