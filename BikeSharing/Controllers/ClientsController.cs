using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BikeSharing.Models;
using Microsoft.AspNetCore.Mvc;

namespace BikeSharing.Controllers
{
    public class ClientsController : Controller
    {
        private ApplicationContext context;

        void setDbContext()
        {
            if (context == null)
                context = HttpContext.RequestServices.
                    GetService(typeof(ApplicationContext)) as ApplicationContext;
        }

        List<Client> GetClients()
        {
            setDbContext();
            List<Client> clients = context.GetAllUsers();
            List<Address> addresses = context.GetAllAddresses();
            List<Passport> passports = context.GetAllPassports();
            List<Name1> surnames = context.GetAllSurnames();
            List<Name2> names = context.GetAllNames();
            List<Name3> patronymics = context.GetAllPatronymics();            
            List<Client> users = (from client in clients
                                  join name in names on client.FirstNameId equals name.Id
                                  join surname in surnames on client.LastNameId equals surname.Id
                                  //join patronymic in patronymics on client.PatronymicId equals patronymic.Id                                  
                                  //join address in addresses on client.AddressId equals address.Id
                                  //join passport in passports on client.PassportId equals passport.Id
                                  select new Client
                                  {
                                      Id = client.Id,
                                      FirstName = name,
                                      LastName = surname,
                                      //Patronymic = patronymic,
                                      //Address = address,
                                      //Passport = passport,
                                      Money = client.Money,
                                      Email = client.Email,
                                      PhoneNumber = client.PhoneNumber

                                  }).ToList<Client>();
            return users;
        }
        public IActionResult Index()
        {   
            return View(GetClients());
        }
    }
}