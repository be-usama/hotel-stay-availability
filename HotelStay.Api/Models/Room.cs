namespace HotelStay.Api.Models;

public class Room
{
    public string RoomId { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public decimal PerNightRate { get; set; }
    public decimal TotalStayPrice { get; set; }
    public CancellationPolicy CancellationPolicy { get; set; }
    public string? StarRating { get; set; }
    public List<string>? Amenities { get; set; }
    public int NumberOfNights { get; set; }
}
