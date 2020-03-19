using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using xero.apis.practice.common.Contracts;
using xero.apis.practice.common.Services;
using xero.apis.practice.web.Models;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;

namespace xero.apis.practice.web.Controllers
{
    public class HomeController : Controller
    {        

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Connected");
            }

            return View();
        }

        [HttpGet]
        public IActionResult SignIn()
        {
            return RedirectToAction("Connected");
        }

        [Authorize]
        public IActionResult Connected()
        {
            return View();
        }
    }
}
