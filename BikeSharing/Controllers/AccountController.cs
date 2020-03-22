using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BikeSharing.Models;
using BikeSharing.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BikeSharing.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext context;

        public AccountController()
        {
            if (context == null)
                context = HttpContext.RequestServices.
                        GetService(typeof(ApplicationContext)) as ApplicationContext;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                Client client = context.Login(model.Email, model.Password);
                if (client != null)
                {
                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                Client client = context.Login(model.Email, model.Password);
                if (client == null)
                {

                }
            }
            else
                ModelState.AddModelError("", "Некорректные логин и(или) пароль");
            return View(model);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}