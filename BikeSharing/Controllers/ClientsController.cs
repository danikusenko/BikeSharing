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

        void setEmptyValues(SearchClientViewModel model)
        {
            model.EmailSearch = model.EmailSearch ?? "";
            model.CitySearch = model.CitySearch ?? "";
            model.CountrySearch = model.CountrySearch ?? "";
            model.NameSearch = model.NameSearch ?? "";
            model.SurnameSearch = model.SurnameSearch ?? "";
            model.PatronymicSearch = model.PatronymicSearch ?? "";
            model.PhoneSearch = model.PhoneSearch ?? "";
        }

        public IActionResult Index(SearchClientViewModel model)
        {
            setDbContext();
            setEmptyValues(model);
            var clients = context.GetAllClients(model);            
            model.Cities = new SelectList(context.GetAllCities());
            model.Countries = new SelectList(context.GetAllCountries());
            model.Clients = new List<Client>(clients);
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
        public IActionResult Block(int unit, BlockingViewModel model, string blocking)
        {
            setDbContext();
            model.BeginningDate = DateTime.Now;
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