using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.ViewModels
{
    public class BlockingViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool? Permanently { get; set; }
        public DateTime? ExpirationDate { get; set; }

        /*[BindProperty]
        public string Gender { get; set; };
        public string[] Genders = new[] { "Male", "Female", "Unspecified" };*/
    }
}

