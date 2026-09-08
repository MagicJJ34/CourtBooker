namespace CourtBooker.Models;

public class Court
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surface { get; set; } = string.Empty;
    public decimal PricePerHour { get; set; }

    public ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}