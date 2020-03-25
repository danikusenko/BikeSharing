using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class Blocking
    {
        public int Id { get; set; }
        public bool Permanently { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
