using System.Text;
using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

[Authorize(Roles = "Admin")]
public class ReportsController : Controller
{
    private readonly AppDbContext _context;

    public ReportsController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("Nieprawidłowy zakres dat.");

        var reservations = await GetReservations(from, to);

        ViewBag.From = from;
        ViewBag.To = to;
        ViewBag.Count = reservations.Count;
        ViewBag.Cancelled = reservations.Count(r => r.InternalStatus == "Cancelled");
        ViewBag.Revenue = reservations
            .Where(r => r.InternalStatus != "Cancelled")
            .Sum(r => r.TotalPrice);

        return View(reservations);
    }

    public async Task<IActionResult> Download(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            return BadRequest("Nieprawidłowy zakres dat.");

        var reservations = await GetReservations(from, to);

        var csv = new StringBuilder();
        csv.AppendLine("Id;Kort;Uzytkownik;Email;Data;Od;Do;Status;Kwota");

        foreach (var r in reservations)
        {
            csv.AppendLine(string.Join(";", new[]
            {
                r.Id.ToString(),
                Csv(r.Court?.Name),
                Csv(r.User?.Name),
                Csv(r.User?.Email),
                r.Date.ToString("yyyy-MM-dd"),
                r.StartTime.ToString("HH:mm"),
                r.EndTime.ToString("HH:mm"),
                Csv(r.Status),
                r.TotalPrice.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)
            }));
        }

        var bytes = new UTF8Encoding(true).GetBytes(csv.ToString());

        return File(bytes, "text/csv", "CourtBooker_Raport.csv");
    }

    private async Task<List<Reservation>> GetReservations(
        DateOnly? from, DateOnly? to)
    {
        var query = _context.Reservations
            .Include(r => r.Court)
            .Include(r => r.User)
            .AsQueryable();

        if (from.HasValue)
            query = query.Where(r => r.Date >= from.Value);

        if (to.HasValue)
            query = query.Where(r => r.Date <= to.Value);

        return await query.OrderByDescending(r => r.Date)
            .ThenBy(r => r.StartTime).ToListAsync();
    }

    private static string Csv(string? value)
    {
        var text = value ?? "";
        return "\"" + text.Replace("\"", "\"\"")
            .Replace("\r", " ").Replace("\n", " ") + "\"";
    }
}
