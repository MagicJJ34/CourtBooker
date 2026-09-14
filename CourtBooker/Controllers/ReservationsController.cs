using System.Security.Claims;
using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

[Authorize]
public class ReservationsController : Controller
{
    private readonly AppDbContext _context;

    public ReservationsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        IQueryable<Reservation> query = _context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User);

        if (!User.IsInRole("Admin"))
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                query = query.Where(r => r.UserId == currentUserId);
            }
        }

        var reservations = await query
            .OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime)
            .ToListAsync();

        return View(reservations);
    }

    public async Task<IActionResult> Create()
    {
        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name");

        if (User.IsInRole("Admin"))
        {
            ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("CourtId,UserId,Date,StartTime,EndTime")] Reservation reservation)
    {
        ModelState.Remove("Court");
        ModelState.Remove("User");

        if (!User.IsInRole("Admin"))
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdClaim, out int currentUserId))
            {
                reservation.UserId = currentUserId;
            }
        }

        if (reservation.Date < DateOnly.FromDateTime(DateTime.Now))
        {
            ModelState.AddModelError("Date", "Nie można rezerwować terminów z przeszłości.");
        }

        if (reservation.StartTime >= reservation.EndTime)
        {
            ModelState.AddModelError("EndTime", "Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.");
        }

        bool isOccupied = await _context.Reservations.AnyAsync(r =>
            r.CourtId == reservation.CourtId &&
            r.Date == reservation.Date &&
            r.InternalStatus != "Cancelled" &&
            reservation.StartTime < r.EndTime &&
            reservation.EndTime > r.StartTime
        );

        if (isOccupied)
        {
            ModelState.AddModelError("", "Kort jest już zarezerwowany w wybranym dniu i godzinach!");
        }

        if (ModelState.IsValid)
        {
            var court = await _context.Courts.FindAsync(reservation.CourtId);
            if (court != null)
            {
                reservation.TotalPrice = reservation.CalculatePrice(court.PricePerHour);
            }

            _context.Add(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name", reservation.CourtId);
        if (User.IsInRole("Admin"))
        {
            ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email", reservation.UserId);
        }
        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && reservation.UserId.ToString() != userIdClaim)
        {
            return Forbid();
        }

        reservation.Status = "Cancelled";
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin") && reservation.UserId.ToString() != userIdClaim)
        {
            return Forbid();
        }

        if (reservation.InternalStatus == "Cancelled")
        {
            return BadRequest("Nie można edytować anulowanej rezerwacji.");
        }

        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name", reservation.CourtId);
        if (User.IsInRole("Admin"))
        {
            ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email", reservation.UserId);
        }

        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,CourtId,UserId,Date,StartTime,EndTime,Status")] Reservation reservation)
    {
        if (id != reservation.Id) return NotFound();

        ModelState.Remove("Court");
        ModelState.Remove("User");

        var existingReservation = await _context.Reservations.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
        if (existingReservation == null) return NotFound();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!User.IsInRole("Admin"))
        {
            if (existingReservation.UserId.ToString() != userIdClaim)
            {
                return Forbid();
            }
            reservation.UserId = existingReservation.UserId;
        }

        if (reservation.Date < DateOnly.FromDateTime(DateTime.Now))
        {
            ModelState.AddModelError("Date", "Nie można przenieść rezerwacji na termin z przeszłości.");
        }

        if (reservation.StartTime >= reservation.EndTime)
        {
            ModelState.AddModelError("EndTime", "Godzina zakończenia musi być późniejsza niż godzina rozpoczęcia.");
        }

        bool isOccupied = await _context.Reservations.AnyAsync(r =>
            r.Id != reservation.Id &&
            r.CourtId == reservation.CourtId &&
            r.Date == reservation.Date &&
            r.InternalStatus != "Cancelled" &&
            reservation.StartTime < r.EndTime &&
            reservation.EndTime > r.StartTime
        );

        if (isOccupied)
        {
            ModelState.AddModelError("", "Wybrany kort jest już zarezerwowany w tych godzinach!");
        }

        if (ModelState.IsValid)
        {
            var court = await _context.Courts.FindAsync(reservation.CourtId);
            if (court != null)
            {
                reservation.TotalPrice = reservation.CalculatePrice(court.PricePerHour);
            }

            _context.Update(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        ViewBag.CourtId = new SelectList(await _context.Courts.ToListAsync(), "Id", "Name", reservation.CourtId);
        if (User.IsInRole("Admin"))
        {
            ViewBag.UserId = new SelectList(await _context.Users.ToListAsync(), "Id", "Email", reservation.UserId);
        }

        return View(reservation);
    }
}