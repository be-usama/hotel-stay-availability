namespace HotelStay.Api.Models;

public class HotelSearchQuery
{
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public RoomType? RoomType { get; set; }
}

public class SearchResponse
{
    public List<Room> Rooms { get; set; } = new();
    public string Destination { get; set; } = string.Empty;
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}
