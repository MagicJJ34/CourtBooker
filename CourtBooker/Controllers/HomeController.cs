using Microsoft.AspNetCore.Mvc;

namespace CourtBooker.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}