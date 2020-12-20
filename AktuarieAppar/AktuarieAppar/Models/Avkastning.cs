using System;

namespace AktuarieAppar.Models.TAT
{
    public partial class Avkastning
    {
        public DateTime ToDate { get; set; }
        public string NodeName { get; set; }
        public decimal MarketValueUB { get; set; }
        public decimal ReturnSekYtd { get; set; }
        public DateTime ValidDate { get; set; }
    }
}
