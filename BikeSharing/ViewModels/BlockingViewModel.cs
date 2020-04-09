using System;

namespace BikeSharing.ViewModels
{
    public class BlockingViewModel
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public bool? Permanently { get; set; }
        public DateTime BeginningDate { get; set; }
        public DateTime? ExpirationDate { get; set; }        
    }
}

