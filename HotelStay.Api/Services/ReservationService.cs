namespace HotelStay.Api.Services;

using HotelStay.Api.Models;

public class ReservationService
{
    private readonly HotelSearchService _searchService;
    private readonly Dictionary<string, ReservationConfirmation> _reservations = new();
    private int _referenceCounter = 0;

    public ReservationService(HotelSearchService searchService)
    {
        _searchService = searchService;
    }

    public async Task<ReservationConfirmation> ReserveRoom(ReservationRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.RoomId))
            throw new ArgumentException("RoomId is required");

        if (string.IsNullOrWhiteSpace(request.GuestName))
            throw new ArgumentException("GuestName is required");

        if (string.IsNullOrWhiteSpace(request.DocumentNumber))
            throw new ArgumentException("DocumentNumber is required");

        var validationError = DestinationValidator.ValidateDocumentForDestination(
            request.Destination, 
            request.DocumentType);

        if (!string.IsNullOrEmpty(validationError))
            throw new InvalidOperationException(validationError);

        var searchQuery = new HotelSearchQuery
        {
            Destination = request.Destination,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut
        };

        var searchResult = await _searchService.SearchHotels(searchQuery);
        var room = searchResult.Rooms.FirstOrDefault(r => r.RoomId == request.RoomId);

        if (room == null)
            throw new KeyNotFoundException($"Room not found: {request.RoomId}");

        var referenceNumber = GenerateReferenceNumber();

        var confirmation = new ReservationConfirmation
        {
            ReferenceNumber = referenceNumber,
            GuestName = request.GuestName,
            ProviderName = room.ProviderName,
            RoomType = room.RoomType,
            CheckIn = request.CheckIn,
            CheckOut = request.CheckOut,
            TotalPrice = room.TotalStayPrice,
            CancellationPolicy = room.CancellationPolicy,
            Status = "Confirmed"
        };

        _reservations[referenceNumber] = confirmation;
        return confirmation;
    }

    public ReservationConfirmation? GetReservation(string referenceNumber)
    {
        return _reservations.TryGetValue(referenceNumber, out var reservation) ? reservation : null;
    }

    private string GenerateReferenceNumber()
    {
        _referenceCounter++;
        var today = DateTime.UtcNow.ToString("yyyyMMdd");
        return $"REF-{today}-{_referenceCounter:D3}";
    }
}
