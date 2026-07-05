using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using HotelStay.Api.Models;
using HotelStay.Api.Services;
using HotelStay.Tests.TestDoubles;

namespace HotelStay.Tests;

public class ReservationServiceTests
{
    private static ReservationService BuildService(params HotelStay.Api.Providers.IHotelProvider[] providers)
    {
        var searchService = new HotelSearchService(providers);
        return new ReservationService(searchService);
    }

    private static Room MakeRoom(string roomId, RoomType type = RoomType.Standard, decimal rate = 100m) => new()
    {
        RoomId = roomId,
        ProviderName = "Alpha",
        RoomType = type,
        PerNightRate = rate,
        TotalStayPrice = rate * 2,
        CancellationPolicy = CancellationPolicy.FreeCancellation,
        NumberOfNights = 2
    };

    private static ReservationRequest ValidRequest(string roomId = "room-1", string destination = "New York") => new()
    {
        RoomId = roomId,
        GuestName = "Jane Doe",
        DocumentType = DocumentType.NationalId,
        DocumentNumber = "N123456",
        Destination = destination,
        CheckIn = DateTime.Today.AddDays(1),
        CheckOut = DateTime.Today.AddDays(3)
    };

    [Fact]
    public async Task ReserveRoom_ValidRequest_ReturnsConfirmedReservation()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);

        var confirmation = await service.ReserveRoom(ValidRequest());

        Assert.Equal("Confirmed", confirmation.Status);
        Assert.Equal("Jane Doe", confirmation.GuestName);
        Assert.Equal("Alpha", confirmation.ProviderName);
        Assert.StartsWith("REF-", confirmation.ReferenceNumber);
    }

    [Fact]
    public async Task ReserveRoom_GeneratesUniqueSequentialReferenceNumbers()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);

        var first = await service.ReserveRoom(ValidRequest());
        var second = await service.ReserveRoom(ValidRequest());

        Assert.NotEqual(first.ReferenceNumber, second.ReferenceNumber);
    }

    [Fact]
    public async Task ReserveRoom_StoresReservationForLaterRetrieval()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);

        var confirmation = await service.ReserveRoom(ValidRequest());
        var retrieved = service.GetReservation(confirmation.ReferenceNumber);

        Assert.NotNull(retrieved);
        Assert.Equal(confirmation.ReferenceNumber, retrieved!.ReferenceNumber);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReserveRoom_MissingRoomId_ThrowsArgumentException(string? roomId)
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest();
        request.RoomId = roomId!;

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveRoom(request));
        Assert.Contains("RoomId is required", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReserveRoom_MissingGuestName_ThrowsArgumentException(string? guestName)
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest();
        request.GuestName = guestName!;

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveRoom(request));
        Assert.Contains("GuestName is required", ex.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ReserveRoom_MissingDocumentNumber_ThrowsArgumentException(string? documentNumber)
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest();
        request.DocumentNumber = documentNumber!;

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => service.ReserveRoom(request));
        Assert.Contains("DocumentNumber is required", ex.Message);
    }

    [Fact]
    public async Task ReserveRoom_InternationalDestinationWithoutPassport_ThrowsInvalidOperationException()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest(destination: "Paris");
        request.DocumentType = DocumentType.NationalId;

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReserveRoom(request));
        Assert.Contains("requires Passport", ex.Message);
    }

    [Fact]
    public async Task ReserveRoom_UnknownDestination_ThrowsInvalidOperationException()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest(destination: "Atlantis");

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReserveRoom(request));
        Assert.Contains("Unknown destination", ex.Message);
    }

    [Fact]
    public async Task ReserveRoom_RoomNotFound_ThrowsKeyNotFoundException()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest(roomId: "does-not-exist");

        var ex = await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ReserveRoom(request));
        Assert.Contains("does-not-exist", ex.Message);
    }

    [Fact]
    public async Task ReserveRoom_NoProvidersReturnRooms_ThrowsKeyNotFoundException()
    {
        var service = BuildService(new FakeHotelProvider());
        var request = ValidRequest();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ReserveRoom(request));
    }

    [Fact]
    public void GetReservation_UnknownReference_ReturnsNull()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);

        var result = service.GetReservation("REF-DOES-NOT-EXIST");

        Assert.Null(result);
    }

    [Fact]
    public async Task ReserveRoom_InternationalDestinationWithPassport_Succeeds()
    {
        var provider = new FakeHotelProvider(MakeRoom("room-1"));
        var service = BuildService(provider);
        var request = ValidRequest(destination: "London");
        request.DocumentType = DocumentType.Passport;

        var confirmation = await service.ReserveRoom(request);

        Assert.Equal("Confirmed", confirmation.Status);
    }
}
