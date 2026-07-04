namespace HotelStay.Api.Models;

public class ReservationRequest
{
    public string RoomId { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public DocumentType DocumentType { get; set; }
    public string DocumentNumber { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}
