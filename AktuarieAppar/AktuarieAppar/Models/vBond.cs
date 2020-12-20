using System;
using System.ComponentModel.DataAnnotations;


namespace AktuarieAppar.Models.SGB
{
    public class vBond
    {
        [DataType(DataType.Date)]
        public DateTime PRICE_DATE { get; set; }
        public Decimal PRICE { get; set; }
        public string SEC_SHORT_NAME { get; set; }
    }
}
