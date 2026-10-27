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

    public async Task<IActionResult> Index(
        int? courtId,
        string period = "30",
        DateOnly? from = null,
        DateOnly? to = null)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (period != "7" && period != "30" &&
            period != "90" && period != "custom")
        {
            period = "30";
        }

        var startDate = period switch
        {
            "7" => today.AddDays(-6),
            "90" => today.AddDays(-89),
            "custom" => from ?? today.AddDays(-29),
            _ => today.AddDays(-29)
        };

        var endDate = period == "custom"
            ? to ?? today
            : today;

        if (startDate > endDate)
        {
            return RedirectToAction(nameof(Index), new
            {
                courtId,
                period = "30"
            });
        }

        var courts = await _context.Courts
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new DashboardCourtOption
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToListAsync();

        if (courtId.HasValue &&
            !courts.Any(c => c.Id == courtId.Value))
        {
            courtId = null;
        }

        var query = _context.Reservations
            .AsNoTracking()
            .Where(r => r.Date >= startDate &&
                        r.Date <= endDate);

        if (courtId.HasValue)
        {
            query = query.Where(r => r.CourtId == courtId.Value);
        }

        var reservations = await query
            .Include(r => r.User)
            .Include(r => r.Court)
            .OrderByDescending(r => r.Date)
            .ThenByDescending(r => r.StartTime)
            .ToListAsync();

        var nonCancelled = reservations
            .Where(r => r.InternalStatus != "Cancelled")
            .ToList();

        var totalReservations = reservations.Count;
        var cancelledReservations = reservations.Count(
            r => r.InternalStatus == "Cancelled"
        );

        var reservationsValue = nonCancelled.Sum(
            r => r.TotalPrice
        );

        var model = new DashboardViewModel
        {
            SelectedCourtId = courtId,
            SelectedPeriod = period,
            FromDate = startDate,
            ToDate = endDate,
            CourtOptions = courts,

            TotalReservations = totalReservations,

            TodayReservations = nonCancelled.Count(
                r => r.Date == today
            ),

            ActiveUsers = await _context.Users.CountAsync(
                u => u.IsActive
            ),

            TotalCourts = courts.Count,

            CancelledReservations = cancelledReservations,

            ReservationsValue = reservationsValue,

            ReservedHours = nonCancelled.Sum(
                r => (decimal)(
                    r.EndTime.ToTimeSpan() -
                    r.StartTime.ToTimeSpan()
                ).TotalHours
            ),

            CancellationRate = totalReservations > 0
                ? Math.Round(
                    cancelledReservations * 100m / totalReservations,
                    1
                )
                : 0m,

            AverageReservationValue = nonCancelled.Count > 0
                ? Math.Round(
                    reservationsValue / nonCancelled.Count,
                    2
                )
                : 0m,

            RecentReservations = reservations
                .Take(8)
                .ToList()
        };

        var courtsForRanking = courtId.HasValue
            ? courts.Where(c => c.Id == courtId.Value).ToList()
            : courts;

        foreach (var court in courtsForRanking)
        {
            var courtReservations = reservations
                .Where(r => r.CourtId == court.Id)
                .ToList();

            var validReservations = courtReservations
                .Where(r => r.InternalStatus != "Cancelled")
                .ToList();

            model.CourtStatistics.Add(new DashboardCourtStatistic
            {
                Name = court.Name,

                Reservations = validReservations.Count,

                CancelledReservations = courtReservations.Count(
                    r => r.InternalStatus == "Cancelled"
                ),

                Hours = validReservations.Sum(
                    r => (decimal)(
                        r.EndTime.ToTimeSpan() -
                        r.StartTime.ToTimeSpan()
                    ).TotalHours
                ),

                Value = validReservations.Sum(
                    r => r.TotalPrice
                )
            });
        }

        model.CourtStatistics = model.CourtStatistics
            .OrderByDescending(c => c.Reservations)
            .ThenBy(c => c.Name)
            .ToList();

        model.MostPopularCourt = model.CourtStatistics
            .FirstOrDefault(c => c.Reservations > 0)?.Name
            ?? "Brak danych";

        var culture = CultureInfo.GetCultureInfo("pl-PL");
        var days = endDate.DayNumber - startDate.DayNumber + 1;

        if (days <= 31)
        {
            for (var date = startDate;
                 date <= endDate;
                 date = date.AddDays(1))
            {
                model.Months.Add(new DashboardMonth
                {
                    Name = date.ToString("dd.MM", culture),
                    Count = nonCancelled.Count(r => r.Date == date)
                });
            }
        }
        else
        {
            var month = new DateOnly(
                startDate.Year,
                startDate.Month,
                1
            );

            var lastMonth = new DateOnly(
                endDate.Year,
                endDate.Month,
                1
            );

            while (month <= lastMonth)
            {
                var currentMonth = month;

                model.Months.Add(new DashboardMonth
                {
                    Name = currentMonth.ToString("MMM yy", culture),

                    Count = nonCancelled.Count(
                        r => r.Date.Year == currentMonth.Year &&
                             r.Date.Month == currentMonth.Month
                    )
                });

                month = month.AddMonths(1);
            }
        }

        for (var hour = 6; hour < 24; hour += 2)
        {
            var start = new TimeOnly(hour, 0);
            var end = hour + 2 == 24
                ? TimeOnly.MinValue
                : new TimeOnly(hour + 2, 0);

            var count = nonCancelled.Count(r =>
                r.StartTime < (hour + 2 == 24
                    ? TimeOnly.MaxValue
                    : end)
                &&
                r.EndTime > start
            );

            model.HourStatistics.Add(new DashboardHourStatistic
            {
                Name = $"{hour:00}:00–{hour + 2:00}:00",
                Count = count
            });
        }

        return View(model);
    }
}
