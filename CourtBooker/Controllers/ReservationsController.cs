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
        var query = _context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .AsQueryable();

        if (!User.IsInRole("Admin"))
        {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            query = query.Where(r => r.UserId == userId);
        }

        return View(await query.OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime).ToListAsync());
    }

    public async Task<IActionResult> Calendar()
    {
        ViewBag.CourtId = new SelectList(
            await _context.Courts.ToListAsync(), "Id", "Name");
        return View();
    }

    public async Task<IActionResult> Create(
        string? date, string? startTime, string? endTime, int? courtId)
    {
        await LoadLists(courtId);

        var reservation = new Reservation();

        if (DateOnly.TryParse(date, out var d))
            reservation.Date = d;

        if (TimeOnly.TryParse(startTime, out var s))
            reservation.StartTime = s;

        if (TimeOnly.TryParse(endTime, out var e))
            reservation.EndTime = e;

        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind("CourtId,UserId,Date,StartTime,EndTime")] Reservation reservation)
    {
        ModelState.Remove("Court");
        ModelState.Remove("User");

        if (!User.IsInRole("Admin"))
        {
            reservation.UserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        if (!await _context.Users.AnyAsync(u => u.Id == reservation.UserId && u.IsActive))
            ModelState.AddModelError("UserId", "Wybierz użytkownika.");

        var court = await _context.Courts.FindAsync(reservation.CourtId);

        if (court == null)
            ModelState.AddModelError("CourtId", "Wybierz kort.");

        await ValidateReservation(reservation);

        if (ModelState.IsValid)
        {
            reservation.InternalStatus = "Pending";
            reservation.TotalPrice = reservation.CalculatePrice(court!.PricePerHour);
            _context.Reservations.Add(reservation);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została utworzona.";
            return RedirectToAction(nameof(Index));
        }

        await LoadLists(reservation.CourtId, reservation.UserId);
        return View(reservation);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
            return NotFound();

        if (!CanAccess(reservation))
            return Forbid();

        if (!CanManage(reservation))
        {
            TempData["Error"] = "Nie można edytować tej rezerwacji.";
            return RedirectToAction(nameof(Index));
        }

        await LoadLists(reservation.CourtId, reservation.UserId);
        return View(reservation);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        [Bind("Id,CourtId,UserId,Date,StartTime,EndTime")] Reservation model)
    {
        if (id != model.Id)
            return NotFound();

        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
            return NotFound();

        if (!CanAccess(reservation))
            return Forbid();

        if (!CanManage(reservation))
        {
            TempData["Error"] = "Nie można edytować tej rezerwacji.";
            return RedirectToAction(nameof(Index));
        }

        ModelState.Remove("Court");
        ModelState.Remove("User");

        if (!User.IsInRole("Admin"))
            model.UserId = reservation.UserId;

        if (!await _context.Users.AnyAsync(u => u.Id == model.UserId && u.IsActive))
            ModelState.AddModelError("UserId", "Wybierz użytkownika.");

        var court = await _context.Courts.FindAsync(model.CourtId);

        if (court == null)
            ModelState.AddModelError("CourtId", "Wybierz kort.");

        await ValidateReservation(model, id);

        if (ModelState.IsValid)
        {
            reservation.CourtId = model.CourtId;
            reservation.UserId = model.UserId;
            reservation.Date = model.Date;
            reservation.StartTime = model.StartTime;
            reservation.EndTime = model.EndTime;
            reservation.TotalPrice = model.CalculatePrice(court!.PricePerHour);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Rezerwacja została zaktualizowana.";
            return RedirectToAction(nameof(Index));
        }

        await LoadLists(model.CourtId, model.UserId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);

        if (reservation == null)
            return NotFound();

        if (!CanAccess(reservation))
            return Forbid();

        if (!CanManage(reservation))
        {
            TempData["Error"] = "Nie można anulować tej rezerwacji.";
            return RedirectToAction(nameof(Index));
        }

        reservation.InternalStatus = "Cancelled";
        await _context.SaveChangesAsync();

        TempData["Success"] = "Rezerwacja została anulowana.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> GetEvents(int? courtId)
    {
        var query = _context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .Where(r => r.InternalStatus != "Cancelled" &&
                        r.InternalStatus != "Anulowano");

        if (courtId.HasValue && courtId.Value > 0)
        {
            query = query.Where(r => r.CourtId == courtId.Value);
        }

        var reservations = await query
            .AsNoTracking()
            .ToListAsync();

        bool isAdmin = User.IsInRole("Admin");

        int currentUserId = int.Parse(
            User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        return Json(reservations.Select(r =>
        {
            bool isOwner = r.UserId == currentUserId;

            string title;

            if (isAdmin)
            {
                title = $"{r.Court?.Name ?? "Kort"} - {r.User?.Name ?? "Użytkownik"}";
            }
            else if (isOwner)
            {
                title = $"{r.Court?.Name ?? "Kort"} - Twoja rezerwacja";
            }
            else
            {
                title = $"{r.Court?.Name ?? "Kort"} - Zajęty";
            }

            return new
            {
                id = r.Id,
                title = title,
                start = $"{r.Date:yyyy-MM-dd}T{r.StartTime:HH\\:mm\\:ss}",
                end = $"{r.Date:yyyy-MM-dd}T{r.EndTime:HH\\:mm\\:ss}",
                color = isOwner ? "#198754" : "#dc3545",
                courtId = r.CourtId
            };
        }));
    }

    private bool CanAccess(Reservation reservation)
    {
        return User.IsInRole("Admin") ||
            reservation.UserId.ToString() ==
            User.FindFirstValue(ClaimTypes.NameIdentifier);
    }

    private bool CanManage(Reservation reservation)
    {
        if (reservation.InternalStatus == "Cancelled")
            return false;

        return User.IsInRole("Admin") ||
            reservation.Date.ToDateTime(reservation.StartTime) > DateTime.Now;
    }

    private async Task ValidateReservation(Reservation reservation, int? excludeId = null)
    {
        if (reservation.Date == default ||
            reservation.StartTime == default ||
            reservation.Date.ToDateTime(reservation.StartTime) <= DateTime.Now)
        {
            ModelState.AddModelError("Date", "Wybierz przyszły termin.");
        }

        if (reservation.StartTime >= reservation.EndTime)
            ModelState.AddModelError("EndTime", "Nieprawidłowe godziny.");

        bool occupied = await _context.Reservations.AnyAsync(r =>
            r.Id != excludeId &&
            r.CourtId == reservation.CourtId &&
            r.Date == reservation.Date &&
            r.InternalStatus != "Cancelled" &&
            reservation.StartTime < r.EndTime &&
            reservation.EndTime > r.StartTime);

        if (occupied)
            ModelState.AddModelError("", "Kort jest już zajęty w tym terminie.");
    }

    private async Task LoadLists(int? courtId = null, int? userId = null)
    {
        ViewBag.CourtId = new SelectList(
            await _context.Courts.ToListAsync(),
            "Id", "Name", courtId);

        if (User.IsInRole("Admin"))
        {
            ViewBag.UserId = new SelectList(
                await _context.Users
                    .Where(u => u.IsActive)
                    .OrderBy(u => u.Email)
                    .ToListAsync(),
                "Id", "Email", userId);
        }
    }
}
