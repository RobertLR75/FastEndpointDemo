# API Documentation

Use Scalar for API docs. It is enabled automatically in Development via `UseEndpoints()`.

- Do not add Swagger UI or Redoc — Scalar is the standard (better UX)
- Available at `/scalar/v1` in development only (not exposed in production)
- OpenAPI spec is registered via `AddOpenApi()` inside `ConfigureFastEndPoints()`
- No additional configuration needed; calling `UseEndpoints()` is sufficient