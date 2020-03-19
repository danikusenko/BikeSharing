using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    [Table("name2")]
    public class Name2
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
    }
}
