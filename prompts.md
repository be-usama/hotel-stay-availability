# AI Prompts Used

This document records how AI was used during implementation, including prompt iterations, key judgment calls, and human review decisions.

## 1) Backend Architecture & Provider Integration

**Initial Prompt**: "Design a hotel availability system with search and reservation endpoints, supporting two stub providers with different response formats (PascalCase vs snake_case)."

**Refined Prompt**: "Use an extensible provider abstraction and aggregate provider results in parallel. Keep provider-specific mapping isolated from API contracts."

**Key Judgment Calls**:
- Chose `IHotelProvider` abstraction to avoid provider logic leaking into endpoints.
- Ran provider calls in parallel for response time.
- Standardized provider models into one API response shape.

**Human Review**:
- Confirmed provider logic remains behind service boundary.
- Confirmed adding a new provider would require minimal API changes.

**Evidence**:
- `HotelStay.Api/Services/HotelSearchService.cs`
- `HotelStay.Api/Program.cs`

## 2) Document Validation Rules

**Initial Prompt**: "Validate travel documents by destination category."

**Refined Prompt**: "Domestic accepts National ID or Passport; international requires Passport. Keep logic centralized and testable."

**Key Judgment Calls**:
- Implemented a dedicated validator (`DestinationValidator`) instead of inline endpoint checks.
- Centralized destination/category mapping to prevent rule duplication.

**Human Review**:
- Verified domestic/international branches match business rules.
- Reviewed invalid-document scenarios for correct API behavior.

**Evidence**:
- `HotelStay.Api/Services/DestinationValidator.cs`
- `HotelStay.Tests/DestinationValidatorTests.cs`

## 3) API Design & Error Semantics

**Initial Prompt**: "Create search, reserve, and lookup endpoints with proper status codes."

**Refined Prompt**: "Use .NET Minimal API style, return 400 for validation problems and 422 for business-rule failures."

**Key Judgment Calls**:
- Kept endpoints thin; moved business logic to services.
- Used explicit status code mapping to separate bad input vs valid-but-unfulfillable requests.

**Human Review**:
- Checked endpoint contracts for predictable frontend consumption.
- Checked negative paths to avoid generic 500 responses for expected rule failures.

**Evidence**:
- `HotelStay.Api/Program.cs`
- `HotelStay.Tests/HotelEndpointsTests.cs`

## 4) Frontend Structure

**Initial Prompt**: "Create Angular components for search and reservation."

**Refined Prompt**: "Use standalone components with service-based API calls and clear user-facing error states."

**Key Judgment Calls**:
- Chose standalone components for simpler module setup.
- Kept API communication in services, not components.

**Human Review**:
- Confirmed form + results + reservation flow is understandable and maintainable.
- Confirmed errors are surfaced in UI flow, not silently ignored.

**Evidence**:
- Frontend search/reservation component and service files.

## 5) Testing Strategy

**Initial Prompt**: "Write meaningful tests for validation and provider calculations."

**Refined Prompt**: "Prioritize business rules and deterministic outcomes over implementation-detail assertions."

**Key Judgment Calls**:
- Focused tests on validation rules, provider aggregation behavior, and reservation edge cases.
- Used deterministic stub data to keep tests stable and repeatable.

**Human Review**:
- Verified tests align with business outcomes rather than internal method structure.
- Verified failure scenarios are covered (not only happy paths).

**Evidence**:
- `HotelStay.Tests/DestinationValidatorTests.cs`
- `HotelStay.Tests/ReservationServiceTests.cs`
- `HotelStay.Tests/HotelEndpointsTests.cs`

## 6) AI Usage Boundaries

**How AI was used**:
- Drafted initial architecture options, endpoint skeletons, and test case ideas.

**How AI was not used**:
- Final business rules, status code mapping, and structure were accepted only after human review.
- No AI output was merged without code-level inspection and adjustment where needed.

**Known Gaps**:
- This log captures major decisions, not every minor prompt iteration.
- Future updates should add commit references for stronger traceability.