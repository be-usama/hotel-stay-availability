using Xunit;
using HotelStay.Api.Models;
using HotelStay.Api.Services;

namespace HotelStay.Tests;

public class DestinationValidatorTests
{
    [Fact]
    public void ValidateDocumentForDestination_International_RequiresPassport()
    {
        var error = DestinationValidator.ValidateDocumentForDestination("Paris", DocumentType.NationalId);
        Assert.Contains("requires Passport", error);
    }

    [Fact]
    public void ValidateDocumentForDestination_International_AcceptsPassport()
    {
        var error = DestinationValidator.ValidateDocumentForDestination("London", DocumentType.Passport);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateDocumentForDestination_Domestic_AcceptsNationalId()
    {
        var error = DestinationValidator.ValidateDocumentForDestination("New York", DocumentType.NationalId);
        Assert.Empty(error);
    }

    [Fact]
    public void ValidateDocumentForDestination_Domestic_AcceptsPassport()
    {
        var error = DestinationValidator.ValidateDocumentForDestination("Los Angeles", DocumentType.Passport);
        Assert.Empty(error);
    }

    [Fact]
    public void GetCategory_ReturnsCorrectCategory()
    {
        Assert.Equal(DestinationCategory.International, DestinationValidator.GetCategory("Paris"));
        Assert.Equal(DestinationCategory.Domestic, DestinationValidator.GetCategory("New York"));
    }
}
