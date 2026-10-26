using System.Globalization;
using CourtBooker.Data;
using CourtBooker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CourtBooker.Controllers;

[Authorize(Roles = "Admin")]
public class DashboardController : Controller
{
    private readonly AppDbContext _context;

    public DashboardController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        var model = new DashboardViewModel
        {
            TotalReservations = await _context.Reservations.CountAsync(),

            TodayReservations = await _context.Reservations.CountAsync(
                r => r.Date == today &&
                     r.InternalStatus != "Cancelled"),

            ActiveUsers = await _context.Users.CountAsync(
                u => u.IsActive),

            TotalCourts = await _context.Courts.CountAsync(),

            CancelledReservations = await _context.Reservations.CountAsync(
                r => r.InternalStatus == "Cancelled"),

            ReservationsValue = await _context.Reservations
                .Where(r => r.InternalStatus != "Cancelled")
                .SumAsync(r => (decimal?)r.TotalPrice) ?? 0
        };

        var popularCourt = await _context.Reservations
            .Where(r => r.InternalStatus != "Cancelled")
            .GroupBy(r => r.CourtId)
            .Select(g => new
            {
                CourtId = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        if (popularCourt != null)
        {
            model.MostPopularCourt = await _context.Courts
                .Where(c => c.Id == popularCourt.CourtId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "Brak danych";
        }

        model.RecentReservations = await _context.Reservations
            .Include(r => r.User)
            .Include(r => r.Court)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .Take(8)
            .AsNoTracking()
            .ToListAsync();

        var firstMonth = new DateOnly(today.Year, today.Month, 1)
            .AddMonths(-5);

        var monthlyDates = await _context.Reservations
            .Where(r => r.Date >= firstMonth &&
                        r.Date <= today &&
                        r.InternalStatus != "Cancelled")
            .Select(r => r.Date)
            .ToListAsync();

        var culture = new CultureInfo("pl-PL");

        for (int i = 0; i < 6; i++)
        {
            var month = firstMonth.AddMonths(i);

            model.Months.Add(new DashboardMonth
            {
                Name = culture.DateTimeFormat
                    .GetAbbreviatedMonthName(month.Month),

                Count = monthlyDates.Count(d =>
                    d.Year == month.Year &&
                    d.Month == month.Month)
            });
        }

        return View(model);
    }
}
