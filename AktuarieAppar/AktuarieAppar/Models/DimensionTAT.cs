using System;
using System.ComponentModel.DataAnnotations;


namespace AktuarieAppar.Models.TAT
{
    public class DimensionTAT
    {
        [DataType(DataType.Date)]
        public DateTime TO_DATE { get; set; }
        public int NUMBER_OF_DAYS { get; set; }
        [Display(Name = "Asset Type")]
        public string NODE_NAME_FOR_REPORTS { get; set; }
        [Display(Name = "MV")]
        public decimal MARKET_VALUE_RC { get; set; }
        [Display(Name = "CF")]
        public decimal NET_CASH_FLOW_RC { get; set; }
        public decimal BALANCE_NOMINAL_NUMBER { get; set; }
        public double TWR_RC { get; set; }
        public DateTime DW_Giltig_from { get; set; }
        public DateTime DW_Giltig_tom { get; set; }
    }
}
