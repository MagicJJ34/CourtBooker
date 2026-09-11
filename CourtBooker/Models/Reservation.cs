using System.ComponentModel.DataAnnotations.Schema;

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

    [Column("Status")]
    public string InternalStatus { get; set; } = "Pending";

    [NotMapped]
    public string Status
    {
        get
        {
            if (InternalStatus == "Cancelled") return "Cancelled";

            var now = DateTime.Now;
            var startDateTime = Date.ToDateTime(StartTime);
            var endDateTime = Date.ToDateTime(EndTime);

            if (now >= endDateTime)
            {
                return "Completed";
            }

            if (now >= startDateTime && now < endDateTime)
            {
                return "In Progress";
            }

            return InternalStatus;
        }
        set
        {
            InternalStatus = value;
        }
    }

    public decimal TotalPrice { get; set; }

    public decimal CalculatePrice(decimal pricePerHour)
    {
        var duration = EndTime - StartTime;
        return (decimal)duration.TotalHours * pricePerHour;
    }
}