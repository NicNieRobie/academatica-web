using Academatica.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace Academatica.Web.Controllers
{
    public class HomeController : Controller
    {
        [Route("email-confirmed")]
        public IActionResult EmailConfirmed()
        {
            return View();
        }

        [Route("email-changed")]
        public IActionResult EmailChanged()
        {
            return View();
        }

        [Route("email-not-confirmed")]
        public IActionResult EmailNotConfirmed()
        {
            return View();
        }

        [Route("email-reverted")]
        public IActionResult EmailReverted()
        {
            return View();
        }

        [Route("error")]
        public IActionResult Error()
        {
            return View();
        }
    }
}
