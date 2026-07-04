# AI Prompts Used

This document tracks AI prompts used during development with notes on key decisions.

## Backend Architecture & Design

**Prompt**: "Design a hotel availability system with search and reservation endpoints, supporting two stub providers with different response formats (PascalCase vs snake_case). Include document validation based on destination type (domestic vs international)."

**Decision**: Implemented Provider pattern with IHotelProvider interface for extensibility. Both providers run in parallel during search.

## Document Validation Logic

**Prompt**: "Create a validation service that validates travel documents based on destination category - domestic allows National ID or Passport, international requires Passport only."

**Decision**: Created DestinationValidator as a static service for reusability across API and tests. Centralized destination mappings.

## Minimal API Endpoints

**Prompt**: "Create RESTful endpoints for hotel search with filtering, room reservation with validation, and reservation lookup. Handle errors with appropriate HTTP status codes (400 for validation, 422 for business logic errors)."

**Decision**: Used .NET 8 Minimal APIs for simplicity. Mapped complex error scenarios to specific HTTP codes.

## Angular Components

**Prompt**: "Create standalone Angular components for hotel search (form + results table) and reservation (form + confirmation). Use services to call backend APIs with proper error handling."

**Decision**: Standalone components for modularity. Implemented shared models/enums between frontend and backend specs.

## Unit Tests

**Prompt**: "Write meaningful xUnit tests covering document validation rules for all destination types, provider data calculation, and error cases."

**Decision**: Tests focus on business logic (validation, pricing calculations) rather than implementation details.

## Provider Stubs

**Prompt**: "Create deterministic stub implementations for PremierStays (full detail: rates, amenities, ratings, star ratings) and BudgetNests (minimal: rates only). Ensure different price points and cancellation policies."

**Decision**: Hard-coded rates per room type for determinism. Each provider has distinct pricing strategy to show differentiation.
