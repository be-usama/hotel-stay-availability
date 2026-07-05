# Hotel Stay Availability

Build a Hotel Availability feature with search, reservation, and document validation.

## Project Structure

- **HotelStay.Api**: .NET 10 Minimal API backend
- **HotelStay.Tests**: xUnit tests
- **hotel-stay-availablity-ui**: Angular frontend

## Setup

### Prerequisites
- .NET 10 SDK
- Node.js 18+ and npm
- Angular CLI (optional, can use `npx ng`)

### Backend Setup

```bash
cd HotelStay.Api
dotnet restore
dotnet build
```

### Frontend Setup

```bash
cd hotel-stay-availablity-ui
npm install
```

## Running the Application

### Start Backend API
```bash
cd HotelStay.Api
dotnet run
# API runs on https://localhost:7101 (or http://localhost:5278)
```

In Development, OpenAPI is available at `https://localhost:7101/openapi/v1.json` and Swagger UI at `https://localhost:7101/swagger`.

### Start Frontend (in new terminal)
```bash
cd hotel-stay-availablity-ui
npm start
# UI runs on http://localhost:4200
```

## Running Tests

```bash
cd HotelStay.Tests
dotnet test
```

## API Endpoints

- `GET /hotels/search?destination={city}&checkIn={date}&checkOut={date}&roomType={type}`
  - Returns available rooms from all providers
  - Query params: destination (required), checkIn (required), checkOut (required), roomType (optional)

- `POST /hotels/reserve`
  - Creates a reservation with document validation
  - Body: { roomId, guestName, documentType, documentNumber, destination, checkIn, checkOut }

- `GET /hotels/reservation/{referenceNumber}`
  - Returns reservation details

## Valid Destinations

**Domestic**: New York, Los Angeles (accepts National ID or Passport)
**International**: London, Paris, Tokyo (requires Passport)

## Assumptions

- No real database persistence; reservations stored in-memory
- No authentication/authorization required
- Stub providers return deterministic, representative data
- All dates in YYYY-MM-DD format
- Rates are per-night; API also returns computed totals for convenience (`totalStayPrice` / `totalPrice`)
