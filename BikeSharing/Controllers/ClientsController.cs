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
using Microsoft.AspNetCore.Mvc.Rendering;

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

        public IActionResult Index(string citySearch, string countrySearch,
            string emailSearch, string phoneSearch, string surnameSearch, string nameSearch,
            string patronymicSearch)
        {
            setDbContext();           
            
            var clients = from client in context.GetAllClients() select client;

            var cities = from city in context.GetAllCities()
                         orderby city.Name
                         select city.Name;
            var countries = from country in context.GetAllCountries()
                            orderby country.Name
                            select country.Name;

            if (!string.IsNullOrEmpty(emailSearch))
            {
                clients = clients.Where(s => s.Email.ToLower().Contains(emailSearch.ToLower()));
            }

            if (!string.IsNullOrEmpty(citySearch))
            {
                clients = clients.Where(x => x.Address.City.Name == citySearch);
            }

            if (!string.IsNullOrEmpty(countrySearch))
            {
                clients = clients.Where(x => x.Address.Country.Name == countrySearch);
            }

            if (!string.IsNullOrEmpty(phoneSearch))
            {
                clients = clients.Where(x => x.PhoneNumber == phoneSearch);
            }

            if (!string.IsNullOrEmpty(surnameSearch))
            {
                clients = clients.Where(x => x.LastName.LastName == surnameSearch);
            }

            if (!string.IsNullOrEmpty(nameSearch))
            {
                clients = clients.Where(x => x.FirstName.FirstName == nameSearch);
            }

            if (!string.IsNullOrEmpty(patronymicSearch))
            {
                clients = clients.Where(x => x.Patronymic.Patronymic == patronymicSearch);
            }

            SearchClientViewModel model = new SearchClientViewModel
            {
                Clients = new List<Client>(clients),
                Cities = new SelectList(cities),
                Countries = new SelectList(countries)
            };
            return View(model);
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
        public IActionResult ChangeRole(string id)
        {
            setDbContext();
            Client client = context.GetClientById(id);
            if (client != null)
            {
                ChangeRoleViewModel model = new ChangeRoleViewModel
                {
                    Id = client.Id,
                    Email = client.Email,
                    UserRole = client.Role
                };
                return View(model);
            }
            return NotFound();
        }

        [HttpPost]
        public IActionResult ChangeRole(string id, string roles)
        {
            setDbContext();
            context.ChangeRole(id, roles);
            return RedirectToAction("Index", "Clients");

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
                    Email = client.Email
                };
                return View(model);
            }
            else
                return NotFound();
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