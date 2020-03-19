using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class Passport
    {
        public int Id { get; set; }

        public int Number { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateIssue { get; set; }

        public string Identification { get; set; }

        [DataType(DataType.Date)]
        public DateTime DateEnd { get; set; }
        public string Series { get; set; }

        public int IssuingPassportId { get; set; }
        public IssuingPassport IssuingPassport { get; set; }

        public int CountryId { get; set; }
        public Country Country { get; set; }
        

        
        
    }
}
