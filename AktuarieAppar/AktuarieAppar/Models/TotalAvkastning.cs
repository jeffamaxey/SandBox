using System;
using System.ComponentModel.DataAnnotations;

namespace AktuarieAppar.Models.TAT
{
    public partial class TotalAvkastning
    {
        public DateTime ToDate { get; set; }
        public string NodeName { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal MarketValueUB { get; set; }
        [DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
        public decimal ReturnSekYtd { get; set; }
        public DateTime ValidDate { get; set; }
    }
}
