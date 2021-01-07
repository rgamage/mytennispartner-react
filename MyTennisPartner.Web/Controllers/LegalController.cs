using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace MyTennisPartner.Web.Controllers
{
    public class LegalController : Controller
    {
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult TermsOfService()
        {
            return View();
        }
    }
}