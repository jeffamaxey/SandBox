using System;

namespace AktuarieAppar.Models.TAT
{
    public partial class PeriodAvkastning
    {
        public DateTime ToDate { get; set; }
        public string NodeName { get; set; }
        public decimal ReturnSekPeriod { get; set; }
        public DateTime InsertDate { get; set; }
    }
}
