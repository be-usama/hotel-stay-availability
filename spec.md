# Hotel Stay Availability - Specification

Data models, enums, interface contracts, and validation rules.

## Data Models

### Room Type Enum
```csharp
public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}
```

### Cancellation Policy Enum
```csharp
public enum CancellationPolicy
{
    FreeCancellation,      // Up to 48h (PremierStays) or 24h (BudgetNests) before check-in
    Flexible,               // Flexible cancellation
    NonRefundable           // Non-refundable
}
```

### Document Type Enum
```csharp
public enum DocumentType
{
    Passport,
    NationalId
}
```

### Destination Category Enum
```csharp
public enum DestinationCategory
{
    Domestic,
    International
}
```

### Room (Available Room)
```csharp
public class Room
{
    public string RoomId { get; set; }                    // Unique identifier
    public string ProviderName { get; set; }             // "PremierStays" or "BudgetNests"
    public RoomType RoomType { get; set; }               // Standard, Deluxe, Suite
    public decimal PerNightRate { get; set; }            // Rate per night
    public decimal TotalStayPrice { get; set; }          // Total for stay duration
    public CancellationPolicy CancellationPolicy { get; set; }
    public string? StarRating { get; set; }              // PremierStays only
    public List<string>? Amenities { get; set; }         // PremierStays only
    public int NumberOfNights { get; set; }              // Computed: checkOut - checkIn
}
```

### Reservation Request
```csharp
public class ReservationRequest
{
    public string RoomId { get; set; }                   // Which room to book
    public string GuestName { get; set; }                // Full name
    public DocumentType DocumentType { get; set; }       // Passport or NationalId
    public string DocumentNumber { get; set; }           // Actual document number
    public string Destination { get; set; }              // For validation
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
}
```

### Reservation Confirmation
```csharp
public class ReservationConfirmation
{
    public string ReferenceNumber { get; set; }         // Unique booking reference
    public string GuestName { get; set; }
    public string ProviderName { get; set; }
    public RoomType RoomType { get; set; }
    public DateTime CheckIn { get; set; }
    public DateTime CheckOut { get; set; }
    public decimal TotalPrice { get; set; }
    public CancellationPolicy CancellationPolicy { get; set; }
    public string Status { get; set; }                   // "Confirmed"
}
```

### Search Query
```csharp
public class HotelSearchQuery
{
    public string Destination { get; set; }              // Required
    public DateTime CheckIn { get; set; }                // Required
    public DateTime CheckOut { get; set; }               // Required
    public RoomType? RoomType { get; set; }              // Optional
}
```

## Destinations

### Domestic Cities (Domestic Destination Category)
- "New York"
- "Los Angeles"

### International Cities (International Destination Category)
- "London"
- "Paris"
- "Tokyo"

**Validation Rule:**
- Domestic → NationalId accepted OR Passport accepted
- International → Passport required ONLY

## API Contracts

### 1. Search Hotels
**Endpoint:** `GET /hotels/search`

**Query Parameters:**
- `destination` (string, required)
- `checkIn` (date, required, format: YYYY-MM-DD)
- `checkOut` (date, required, format: YYYY-MM-DD)
- `roomType` (enum, optional)

**Response (200 OK):**
```json
{
  "rooms": [
    {
      "roomId": "ps-001-deluxe",
      "providerName": "PremierStays",
      "roomType": "Deluxe",
      "perNightRate": 150.00,
      "totalStayPrice": 600.00,
      "cancellationPolicy": "FreeCancellation",
      "starRating": "5",
      "amenities": ["WiFi", "Pool", "Gym"],
      "numberOfNights": 4
    }
  ],
  "destination": "Paris",
  "checkIn": "2026-07-10",
  "checkOut": "2026-07-14"
}
```

**Error Responses:**
- `400 Bad Request` - missing destination, checkIn, checkOut, or checkOut ≤ checkIn
  ```json
  {
    "error": "checkOut must be after checkIn"
  }
  ```

---

### 2. Create Reservation
**Endpoint:** `POST /hotels/reserve`

**Request Body:**
```json
{
  "roomId": "ps-001-deluxe",
  "guestName": "John Doe",
  "documentType": "Passport",
  "documentNumber": "A12345678",
  "destination": "Paris",
  "checkIn": "2026-07-10",
  "checkOut": "2026-07-14"
}
```

**Response (201 Created):**
```json
{
  "referenceNumber": "REF-20260705-001",
  "guestName": "John Doe",
  "providerName": "PremierStays",
  "roomType": "Deluxe",
  "checkIn": "2026-07-10",
  "checkOut": "2026-07-14",
  "totalPrice": 600.00,
  "cancellationPolicy": "FreeCancellation",
  "status": "Confirmed"
}
```

**Error Responses:**
- `422 Unprocessable Entity` - invalid document for destination
  ```json
  {
    "error": "International destination 'Paris' requires Passport, but NationalId provided"
  }
  ```
- `400 Bad Request` - missing required fields
- `404 Not Found` - room not found

---

### 3. Get Reservation Details
**Endpoint:** `GET /hotels/reservation/{referenceNumber}`

**Response (200 OK):**
```json
{
  "referenceNumber": "REF-20260705-001",
  "guestName": "John Doe",
  "providerName": "PremierStays",
  "roomType": "Deluxe",
  "checkIn": "2026-07-10",
  "checkOut": "2026-07-14",
  "totalPrice": 600.00,
  "cancellationPolicy": "FreeCancellation",
  "status": "Confirmed"
}
```

**Error Response:**
- `404 Not Found` - reservation not found

---

## Document Validation Rules

| Destination | Category | Required Document | Also Accepted |
|---|---|---|---|
| New York | Domestic | NationalId | Passport |
| Los Angeles | Domestic | NationalId | Passport |
| London | International | Passport | — |
| Paris | International | Passport | — |
| Tokyo | International | Passport | — |

- **Server-side validation required** on POST /hotels/reserve (422 on mismatch)
- **Client-side validation recommended** for UX

---

## Provider Interfaces

### IHotelProvider
```csharp
public interface IHotelProvider
{
    Task<List<Room>> GetAvailableRooms(
        string destination,
        DateTime checkIn,
        DateTime checkOut,
        RoomType? roomType = null);
}
```

### Provider Implementation Notes

**PremierStays (Full Detail)**
- Response Format: PascalCase JSON
- Fields: RoomType, PerNightRate, StarRating, Amenities, CancellationPolicy
- Availability: Always available
- Stub: Deterministic based on destination + roomType

**BudgetNests (Minimal Detail)**
- Response Format: snake_case JSON
- Fields: room_type, per_night_rate, cancellation_policy
- Availability: May return `"available": false` → filter out
- Stub: Deterministic based on destination + roomType

---

## Business Logic

### Search Flow
1. Validate search query (destination, dates, dates order)
2. Query both providers in parallel
3. Filter out unavailable rooms
4. Normalize responses to unified Room model
5. Sort by provider + room type
6. Return combined list

### Reservation Flow
1. Validate reservation request fields
2. **Validate document against destination**
   - If international: passport required (422 if not)
   - If domestic: national ID accepted; passport also OK
3. Generate unique reference number (REF-YYYYMMDD-###)
4. Store reservation (in-memory for this app)
5. Return confirmation

---

## Assumptions

- No database persistence; all reservations in-memory
- No authentication/authorization
- All dates in YYYY-MM-DD format
- Stub providers return deterministic data (same for same inputs)
- Rates are per-night; UI must calculate total
- Search runs offline; providers are local stubs
