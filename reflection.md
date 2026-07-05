# Reflection

What would be improved with more time.

## Suggested Priority Order

1. **Data persistence first**: Replace in-memory reservations in `ReservationService` with PostgreSQL/SQL Server and add migration scripts.
2. **Quality next**: Add integration tests for search → reserve → lookup and baseline structured logging.
3. **Product polish**: Improve responsiveness/accessibility, then add advanced filters.
4. **Scale-oriented items later**: Add caching, rate limiting, and real-time updates once traffic patterns justify them.

## Architecture & Design

1. **Event-Driven Reservations (Future Scale Option)**: If reservation throughput or cross-service workflows grow, evaluate event-driven processing (e.g., queue + outbox) for better resiliency and traceability.

2. **Provider Abstraction**: Extend to real hotel APIs (Booking.com, Expedia) with adapter patterns, circuit breakers, and rate limiting.

3. **Caching Strategy**: Implement Redis caching for search results with TTL, reducing provider queries and improving response times.

## Data & Persistence

1. **Database Layer**: Replace in-memory reservation storage in `ReservationService` with PostgreSQL/SQL Server. Add migrations and repository pattern for data access.

2. **Concurrency Management**: Add optimistic concurrency / row-version checks and idempotency keys to prevent overbooking under concurrent requests.

3. **Audit Logging**: Track all reservation changes with timestamps, user IDs, and state transitions.

## Frontend Improvements

1. **State Management**: Migrate from component state to NgRx for global state, making multi-step flows (search → reserve → confirmation) more robust.

2. **Advanced Filtering**: Add price range slider, amenity filters, provider preferences, sort options (price, rating, availability).

3. **Mobile Responsive**: Current UI is basic desktop; needs responsive design for mobile/tablet.

4. **Accessibility**: Add ARIA labels, keyboard navigation, contrast improvements for WCAG 2.1 compliance.

5. **Real-time Updates**: WebSocket connection to notify users of price changes or room availability.

## Testing & Quality

1. **Integration Tests**: Add API integration tests simulating end-to-end flows (search → reserve → lookup).

2. **E2E Tests**: Cypress/Playwright tests for full user journeys.

3. **Performance Testing**: Load testing for provider parallelism and concurrent reservations.

4. **Security Testing**: Validate input sanitization, SQL injection prevention, CORS misconfiguration.

## Operational Excellence

1. **API Versioning**: Support multiple API versions for backward compatibility.

2. **Rate Limiting**: Implement per-IP/per-user rate limiting to prevent abuse.

3. **Monitoring & Logging**: Structured logging (Serilog), distributed tracing (OpenTelemetry), metrics (Prometheus).

4. **Documentation**: OpenAPI/Swagger specs with examples, runbook for deployment and troubleshooting.

5. **CI/CD Pipeline**: GitHub Actions for automated testing, code coverage reports, security scanning before deployment.

## Code Quality

1. **SOLID Principles**: Ensure single responsibility, dependency injection, interface segregation throughout.

2. **Error Handling**: Implement custom exception hierarchy with specific error codes for different failure modes.

3. **Validation**: Fluent validation framework for request validation at API boundary.

4. **Comments**: Add XML docs for public APIs, architectural decision records (ADRs) for complex logic.
