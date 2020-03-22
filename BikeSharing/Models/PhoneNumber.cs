using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class PhoneNumber
    {
        public int Id { get; set; }
        
        [DataType(DataType.PhoneNumber)]
        public string Number { get; set; }        
    }
}
