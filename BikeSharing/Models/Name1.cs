using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{

    [Table("name1")]
    public class Name1
    {
        public int Id { get; set; }
        public string LastName { get; set; }
    }
}
