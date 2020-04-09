using BikeSharing.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;

namespace BikeSharing.ViewModels
{
    public class SearchClientViewModel
    {
        public List<Client> Clients { get; set; }

        public SelectList Cities { get; set; }

        public string CitySearch { get; set; }

        public SelectList Countries { get; set; }

        public string CountrySearch { get; set; }

        public string SurnameSearch { get; set; }

        public string NameSearch { get; set; }

        public string PatronymicSearch { get; set; }

        public string PhoneSearch { get; set; }

        public string EmailSearch { get; set; }
    }
}
