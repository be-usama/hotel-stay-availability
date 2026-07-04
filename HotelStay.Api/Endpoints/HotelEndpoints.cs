namespace HotelStay.Api.Endpoints;

using HotelStay.Api.Models;
using HotelStay.Api.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class HotelEndpoints
{
    public static void MapHotelEndpoints(this WebApplication app, HotelSearchService searchService, ReservationService reservationService)
    {
        var group = app.MapGroup("/hotels")
            .WithName("Hotels");

        group.MapGet("/search", SearchHotels(searchService))
            .WithName("SearchHotels");

        group.MapPost("/reserve", ReserveHotel(reservationService))
            .WithName("ReserveHotel");

        group.MapGet("/reservation/{referenceNumber}", GetReservation(reservationService))
            .WithName("GetReservation");
    }

    private static Func<string?, string?, string?, string?, HotelSearchService, Task<IResult>> SearchHotels(HotelSearchService searchService)
    {
        return async (destination, checkIn, checkOut, roomType, service) =>
        {
            if (string.IsNullOrWhiteSpace(destination))
                return Results.BadRequest(new { error = "destination is required" });

            if (!DateTime.TryParse(checkIn, out var checkInDate))
                return Results.BadRequest(new { error = "checkIn must be in YYYY-MM-DD format" });

            if (!DateTime.TryParse(checkOut, out var checkOutDate))
                return Results.BadRequest(new { error = "checkOut must be in YYYY-MM-DD format" });

            if (checkOutDate <= checkInDate)
                return Results.BadRequest(new { error = "checkOut must be after checkIn" });

            RoomType? parsedRoomType = null;
            if (!string.IsNullOrWhiteSpace(roomType) && Enum.TryParse<RoomType>(roomType, true, out var parsed))
                parsedRoomType = parsed;

            var query = new HotelSearchQuery
            {
                Destination = destination,
                CheckIn = checkInDate,
                CheckOut = checkOutDate,
                RoomType = parsedRoomType
            };

            var result = await service.SearchHotels(query);
            return Results.Ok(result);
        };
    }

    private static Func<ReservationRequest, ReservationService, Task<IResult>> ReserveHotel(ReservationService reservationService)
    {
        return async (request, service) =>
        {
            try
            {
                var confirmation = await service.ReserveRoom(request);
                return Results.Created($"/hotels/reservation/{confirmation.ReferenceNumber}", confirmation);
            }
            catch (InvalidOperationException ex)
            {
                return Results.UnprocessableEntity(new { error = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { error = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        };
    }

    private static Func<string, ReservationService, IResult> GetReservation(ReservationService reservationService)
    {
        return (referenceNumber, service) =>
        {
            var reservation = service.GetReservation(referenceNumber);
            if (reservation == null)
                return Results.NotFound(new { error = $"Reservation not found: {referenceNumber}" });

            return Results.Ok(reservation);
        };
    }
}
