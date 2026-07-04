namespace HotelStay.Api.Services;

using HotelStay.Api.Models;

public class DestinationValidator
{
    private static readonly Dictionary<string, DestinationCategory> Destinations = new()
    {
        { "New York", DestinationCategory.Domestic },
        { "Los Angeles", DestinationCategory.Domestic },
        { "London", DestinationCategory.International },
        { "Paris", DestinationCategory.International },
        { "Tokyo", DestinationCategory.International }
    };

    public static DestinationCategory? GetCategory(string destination)
    {
        return Destinations.TryGetValue(destination, out var category) ? category : null;
    }

    public static bool IsValidDestination(string destination)
    {
        return Destinations.ContainsKey(destination);
    }

    public static string ValidateDocumentForDestination(string destination, DocumentType documentType)
    {
        var category = GetCategory(destination);

        if (category == null)
            return $"Unknown destination: {destination}";

        if (category == DestinationCategory.International)
        {
            if (documentType != DocumentType.Passport)
                return $"International destination '{destination}' requires Passport, but {documentType} provided";
        }

        return string.Empty;
    }
}
