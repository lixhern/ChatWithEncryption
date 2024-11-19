using ChatWithEncryption.Data;
using Microsoft.AspNetCore.Mvc;

namespace ChatWithEncryption.Controllers
{
    public class ContactsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactsController(ApplicationDbContext context)
        {
            _context = context;
        }
        public IActionResult Index()
        {
            return View();
        }


    }
}
