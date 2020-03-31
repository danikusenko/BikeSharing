using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BikeSharing.Models;
using BikeSharing.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace BikeSharing.Controllers
{
    [Authorize(Roles = "Администратор")]
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
                                  join patronymic in patronymics on client.PatronymicId equals patronymic.Id
                                  //join address in addresses on client.AddressId equals address.Id
                                  //join passport in passports on client.PassportId equals passport.Id
                                  select new Client
                                  {
                                      Id = client.Id,
                                      FirstName = name,
                                      LastName = surname,
                                      Patronymic = patronymic,
                                      //Address = address,
                                      //Passport = passport,
                                      Money = client.Money,
                                      Email = client.Email,
                                      PhoneNumber = client.PhoneNumber,
                                      BlockingId = client.BlockingId

                                  }).ToList<Client>();
            return users;
        }
        public IActionResult Index()
        {
            return View(GetClients());
        }

        [HttpPost]
        public IActionResult Delete(string id)
        {
            setDbContext();
            context.DeleteUser(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(RegisterModel model)
        {
            setDbContext();
            if (ModelState.IsValid)
            {
                Client client = context.Register(model);
                if (client != null)
                    return RedirectToAction("Index", "Clients");
                else
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Block(string id)
        {
            setDbContext();
            Client client = context.GetClientById(id);
            if (client != null)
            {
                BlockingViewModel model = new BlockingViewModel
                {
                    Id = client.Id,
                    Email = client.Email/*,
                    Permanently = client.Blocking.Permanently,
                    ExpirationDate = client.Blocking.ExpirationDate*/
                };
                return View(model);
            }
            else
                return RedirectToAction("Index", "Clients");
        }

        [HttpPost]
        public async Task<IActionResult> Block(int unit, BlockingViewModel model, string blocking)
        {
            setDbContext();
            if (blocking == "permanently")
                model.Permanently = true;
            else
            {
                model.Permanently = false;
                switch (blocking)
                {
                    case "hours":
                        model.ExpirationDate = DateTime.Now.AddHours(unit);
                        break;
                    case "days":
                        model.ExpirationDate = DateTime.Now.AddDays(unit);
                        break;
                    case "months":
                        model.ExpirationDate = DateTime.Now.AddMonths(unit);
                        break;
                    case "years":
                        model.ExpirationDate = DateTime.Now.AddYears(unit);
                        break;
                }
            }
            context.BlockUser(model);
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.LoginPath("fs"));
            return RedirectToAction("Index", "Clients");
        }

        [HttpPost]
        public IActionResult Unblock(string id)
        {
            setDbContext();
            context.UnblockUser(id);
            return RedirectToAction("Index", "Clients");
        }
    }
}