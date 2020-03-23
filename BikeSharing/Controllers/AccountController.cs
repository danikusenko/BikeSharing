﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BikeSharing.Models;
using BikeSharing.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BikeSharing.Controllers
{
    public class AccountController : Controller
    {
        private ApplicationContext context;

        public AccountController()
        {
            /*if (context == null)
                context = HttpContext.RequestServices.
                        GetService(typeof(ApplicationContext)) as ApplicationContext;*/
        }

        void setDbContext()
        {
            if (context == null)
                context = HttpContext.RequestServices.
                        GetService(typeof(ApplicationContext)) as ApplicationContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            setDbContext();
            if (ModelState.IsValid)
            {
                Client client = context.Login(model);
                if (client != null)
                {
                    await Authenticate(client);
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
        public async Task<IActionResult> Register(RegisterModel model)
        {
            setDbContext();
            if (ModelState.IsValid)
            {
                Client client = context.Register(model);
                if (client != null)
                {
                    await Authenticate(client);
                    return RedirectToAction("Index", "Home");
                }
                else
                {                    
                    ModelState.AddModelError("", "Некорректные логин и(или) пароль");
                }
            }
            return View(model);
        }

        private async Task Authenticate(Client client)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimsIdentity.DefaultNameClaimType, client.Email),
                new Claim(ClaimsIdentity.DefaultRoleClaimType, client.Role)
            };
            ClaimsIdentity id = new ClaimsIdentity(claims, "ApplicationCookie", ClaimsIdentity.DefaultNameClaimType, ClaimsIdentity.DefaultRoleClaimType);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(id));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
    }
}