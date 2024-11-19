using ChatWithEncryption.Data;
using ChatWithEncryption.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ChatWithEncryption.Controllers
{
    public class HomeController : Controller
    {
        //private readonly Context
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }


        public async Task<ActionResult> SearchById(string id)
        {

            //var user = await _context.Users.FindAsync(id);
            var user = await _context.Users.FindAsync(id);

            if (string.IsNullOrEmpty(id))
            {
                return PartialView("Index", null); 
            }

            
            return PartialView("_SearchResult", user);
        }

        //[Authorize]
        public IActionResult Index()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Auth");
            }
            else
            {
                return View();
            }

            
           
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


    }
}
