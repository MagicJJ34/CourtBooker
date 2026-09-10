using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

public class ReservationsController : Controller
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var reservations = await _context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .OrderByDescending(r => r.StartTime)
            .ToListAsync();

        return View(reservations);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name");
        ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourtId,UserId,StartTime,EndTime")] Reservation reservation)
    {
        ModelState.Remove("Court");
        ModelState.Remove("User");

        if (reservation.StartTime >= reservation.EndTime)
        {
            ModelState.AddModelError("EndTime", "Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.");
        }

        bool isOccupied = await _context.Reservations.AnyAsync(r =>
            r.CourtId == reservation.CourtId &&
            reservation.StartTime < r.EndTime &&
            reservation.EndTime > r.StartTime
        );

        if (isOccupied)
        {
            ModelState.AddModelError("", "Kort jest już zarezerwowany w wybranym przedziale czasowym!");
        }

        if (ModelState.IsValid)
        {
            _context.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name", reservation.CourtId);
        ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email", reservation.UserId);
        return View(reservation);
    }
}