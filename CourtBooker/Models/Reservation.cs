namespace CourtBooker.Models;

public class Reservation
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }

    public int CourtId { get; set; }
    public Court? Court { get; set; }

    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Status { get; set; } = "Pending";

    public decimal TotalPrice { get; set; }

    public decimal CalculatePrice(decimal pricePerHour)
    {
        var duration = EndTime - StartTime;
        return (decimal)duration.TotalHours * pricePerHour;
    }
}