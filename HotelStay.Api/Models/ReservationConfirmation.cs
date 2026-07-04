namespace HotelStay.Api.Models;

public class ReservationConfirmation
{
    public string ReferenceNumber { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public CancellationPolicy CancellationPolicy { get; set; }
    public string Status { get; set; } = "Confirmed";
}
