using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

public class CourtsController : Controller
{
    private readonly AppDbContext _context;

    public CourtsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var courts = await _context.Courts.ToListAsync();
        return View(courts);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Id,Name,Surface,PricePerHour")] Court court)
    {
        ModelState.Remove("Reservations");

        if (ModelState.IsValid)
        {
            _context.Add(court);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        return View(court);
    }
}