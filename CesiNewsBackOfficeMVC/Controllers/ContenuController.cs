using CesiNewsBackOfficeMVC.Controllers;
using CESIZenBackOfficeMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Diagnostics;

namespace CESIZenBackOfficeMVC.Controllers
{
    public class ContenuController : Controller
    {
        public IActionResult Contenu()
        {
            return View();
        }

    }
}
