// Controllers/ChatController.cs
using Microsoft.AspNetCore.Mvc;

namespace DigitalLockerSystem.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}