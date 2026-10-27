namespace CourtBooker.Models;

public class DashboardViewModel
{
    public int TotalReservations { get; set; }
    public int TodayReservations { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCourts { get; set; }
    public int CancelledReservations { get; set; }

    public decimal ReservationsValue { get; set; }
    public decimal ReservedHours { get; set; }
    public decimal CancellationRate { get; set; }
    public decimal AverageReservationValue { get; set; }

    public string MostPopularCourt { get; set; } = "Brak danych";

    public List<DashboardMonth> Months { get; set; } = new();
    public List<Reservation> RecentReservations { get; set; } = new();

    public int? SelectedCourtId { get; set; }
    public string SelectedPeriod { get; set; } = "30";
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }

    public List<DashboardCourtOption> CourtOptions { get; set; } = new();
    public List<DashboardCourtStatistic> CourtStatistics { get; set; } = new();
    public List<DashboardHourStatistic> HourStatistics { get; set; } = new();
}

public class DashboardMonth
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}

public class DashboardCourtOption
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class DashboardCourtStatistic
{
    public string Name { get; set; } = "";
    public int Reservations { get; set; }
    public int CancelledReservations { get; set; }
    public decimal Hours { get; set; }
    public decimal Value { get; set; }
}

public class DashboardHourStatistic
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}
