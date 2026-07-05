using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using HotelStay.Api.Models;

namespace HotelStay.Tests;

public class HotelEndpointsTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    // Mirrors the enum handling configured server-side (ConfigureHttpJsonOptions in Program.cs)
    // so HttpClient.ReadFromJsonAsync can deserialize enum values like RoomType correctly.
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public HotelEndpointsTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    private static string ToDateParam(DateTime date) => date.ToString("yyyy-MM-dd");

    [Fact]
    public async Task Search_ValidRequest_ReturnsOkWithRooms()
    {
        var client = _factory.CreateClient();
        var checkIn = ToDateParam(DateTime.Today.AddDays(1));
        var checkOut = ToDateParam(DateTime.Today.AddDays(3));

        var response = await client.GetAsync($"/hotels/search?destination=Paris&checkIn={checkIn}&checkOut={checkOut}");

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SearchResponse>(JsonOptions);
        Assert.NotNull(result);
        Assert.NotEmpty(result!.Rooms);
    }

    [Fact]
    public async Task Search_MissingDestination_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var checkIn = ToDateParam(DateTime.Today.AddDays(1));
        var checkOut = ToDateParam(DateTime.Today.AddDays(3));

        var response = await client.GetAsync($"/hotels/search?checkIn={checkIn}&checkOut={checkOut}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_InvalidCheckInFormat_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var checkOut = ToDateParam(DateTime.Today.AddDays(3));

        var response = await client.GetAsync($"/hotels/search?destination=Paris&checkIn=not-a-date&checkOut={checkOut}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_InvalidCheckOutFormat_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var checkIn = ToDateParam(DateTime.Today.AddDays(1));

        var response = await client.GetAsync($"/hotels/search?destination=Paris&checkIn={checkIn}&checkOut=not-a-date");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_CheckOutBeforeCheckIn_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var checkIn = ToDateParam(DateTime.Today.AddDays(3));
        var checkOut = ToDateParam(DateTime.Today.AddDays(1));

        var response = await client.GetAsync($"/hotels/search?destination=Paris&checkIn={checkIn}&checkOut={checkOut}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Search_InvalidRoomTypeString_IsIgnoredAndReturnsAllTypes()
    {
        var client = _factory.CreateClient();
        var checkIn = ToDateParam(DateTime.Today.AddDays(1));
        var checkOut = ToDateParam(DateTime.Today.AddDays(3));

        var response = await client.GetAsync($"/hotels/search?destination=Paris&checkIn={checkIn}&checkOut={checkOut}&roomType=NotARealType");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task ReserveThenGet_ValidFlow_ReturnsCreatedAndRetrievable()
    {
        var client = _factory.CreateClient();
        var searchResponse = await client.GetFromJsonAsync<SearchResponse>(
            $"/hotels/search?destination=New York&checkIn={ToDateParam(DateTime.Today.AddDays(1))}&checkOut={ToDateParam(DateTime.Today.AddDays(3))}",
            JsonOptions);
        var roomId = searchResponse!.Rooms[0].RoomId;

        var request = new ReservationRequest
        {
            RoomId = roomId,
            GuestName = "John Smith",
            DocumentType = DocumentType.NationalId,
            DocumentNumber = "N999",
            Destination = "New York",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        };

        var reserveResponse = await client.PostAsJsonAsync("/hotels/reserve", request);
        Assert.Equal(HttpStatusCode.Created, reserveResponse.StatusCode);

        var confirmation = await reserveResponse.Content.ReadFromJsonAsync<ReservationConfirmation>(JsonOptions);
        Assert.NotNull(confirmation);

        var getResponse = await client.GetAsync($"/hotels/reservation/{confirmation!.ReferenceNumber}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Reserve_MissingGuestName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = new ReservationRequest
        {
            RoomId = "some-room",
            GuestName = "",
            DocumentType = DocumentType.NationalId,
            DocumentNumber = "N999",
            Destination = "New York",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        };

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Reserve_InternationalDestinationWrongDocument_ReturnsUnprocessableEntity()
    {
        var client = _factory.CreateClient();
        var request = new ReservationRequest
        {
            RoomId = "some-room",
            GuestName = "Jane Doe",
            DocumentType = DocumentType.NationalId,
            DocumentNumber = "N999",
            Destination = "Paris",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        };

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task Reserve_UnknownRoomId_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var request = new ReservationRequest
        {
            RoomId = "does-not-exist",
            GuestName = "Jane Doe",
            DocumentType = DocumentType.NationalId,
            DocumentNumber = "N999",
            Destination = "New York",
            CheckIn = DateTime.Today.AddDays(1),
            CheckOut = DateTime.Today.AddDays(3)
        };

        var response = await client.PostAsJsonAsync("/hotels/reserve", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetReservation_UnknownReference_ReturnsNotFound()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/hotels/reservation/REF-DOES-NOT-EXIST");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
