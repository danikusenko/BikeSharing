using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class Address
    {
        public int Id { get; set; }
        public string HouseNumber { get; set; }
        public string FlatNumber { get; set; }
        public int Building { get; set; }

        public string StreetType { get; set; }

        public string CityType { get; set; }        

        public int CountryId { get; set; }

        public int CityId { get; set; }

        public int RegionId { get; set; }

        public int AreaId { get; set; }

        public int StreetId { get; set; }

        public Country Country { get; set; }

        public City City { get; set; }

        public Region Region { get; set; }

        public Area Area { get; set; }

        public Street Street { get; set; }
    }
}
