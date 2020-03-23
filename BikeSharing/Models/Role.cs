using System.Collections.Generic;

namespace BikeSharing.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<Client> Clients { get; set; }
        public Role()
        {
            Clients = new List<Client>();
        }
    }
}
