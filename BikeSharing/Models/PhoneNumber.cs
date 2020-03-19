using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class PhoneNumber
    {
        public int Id { get; set; }
        public int Number { get; set; }     

        public int CountryCodeId { get; set; }
        public CountryCode CountryCode { get; set; }

        public int OperatorCodeId { get; set; }
        public OperatorCode OperatorCode { get; set; }
    }
}
