namespace CourtBooker.Models;

public class DashboardViewModel
{
    public int TotalReservations { get; set; }
    public int TodayReservations { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalCourts { get; set; }
    public int CancelledReservations { get; set; }

    public decimal ReservationsValue { get; set; }

    public string MostPopularCourt { get; set; } = "Brak danych";

    public List<DashboardMonth> Months { get; set; } = new();

    public List<Reservation> RecentReservations { get; set; } = new();
}

public class DashboardMonth
{
    public string Name { get; set; } = "";
    public int Count { get; set; }
}
