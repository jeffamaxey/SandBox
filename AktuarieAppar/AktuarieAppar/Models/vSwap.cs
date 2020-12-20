using System;
using System.ComponentModel.DataAnnotations;


namespace AktuarieAppar.Models.NSS
{
    public class vSwap
    {
        [DataType(DataType.Date)]
        public DateTime PRICEDATE { get; set; }
        public Decimal QUOTE { get; set; }
        public int ID { get; set; }
        public int PRICETYPE { get; set; }
        public string Name { get; set; }
        public string PTNAME { get; set; }
        public int Maturity { get; set; }
        public DateTime Inläsningsdatum { get; set; }
    }
}
