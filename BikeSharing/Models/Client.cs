using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BikeSharing.Models
{
    public class Client
    {
        public int Id { get; set; }

        public float Money { get; set; }

        public Name2 FirstName { get; set; }
        public int FirstNameId { get; set; }

        public Name1 LastName { get; set; }
        public int LastNameId { get; set; }

        public Name3 Patronymic { get; set; }
        public int PatronymicId { get; set; }

        public PhoneNumber PhoneNumber { get; set; }
        public int PhoneNumberId { get; set; }

        public Address Address { get; set; }
        public int AddressId { get; set; }

        public Passport Passport { get; set; }
        public int PassportId { get; set; }
    }
}
