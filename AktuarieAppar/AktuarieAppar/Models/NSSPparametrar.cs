using System;
using System.ComponentModel.DataAnnotations;


namespace AktuarieAppar.Models.NSS
{
    public partial class NSSParametrar
    {
        public string Grunder { get; set; }
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }
        public string KurvNamn { get; set; }
        public double Betha0 { get; set; }
        public double Betha1 { get; set; }
        public double Betha2 { get; set; }
        public double Betha3 { get; set; }
        public double Tao0 { get; set; }
        public double Tao1 { get; set; }
        public double TKonst { get; set; }
        public double RelativeTax { get; set; }
    }
}
