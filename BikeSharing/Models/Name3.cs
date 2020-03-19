using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    [Table("name3")]
    public class Name3
    {
        public int Id { get; set; }
        public string Patronymic { get; set; }
    }
}
