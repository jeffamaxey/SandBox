
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;


namespace AktuarieAppar.Models
{
    public partial class LabanGrund
    {   
        public string Miljo { get; set; }
        
        public string Bolag { get; set; }
        
        public string Gren { get; set; }
        
        public string Skatt { get; set; }

        public string Grunder { get; set; }
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        public double Delta { get; set; }
        public double EpsDelta { get; set; }
    }
}
