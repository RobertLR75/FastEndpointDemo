# Machine-to-Machine (M2M) Authentication

All service-to-service HTTP calls use OAuth2 client credentials flow via Duende Access Token Management.

```csharp
// Program.cs
builder.ConfigureClientCredentialsTokenManagement(new IdentityClientSettings
{
    Name = "order-service-client",
    ClientId = "...",
    ClientSecret = "...",
    TokenEndpoint = new Uri("https://identity-server/connect/token"),
    Scopes = "orders.read orders.write"
});
```

- Configure in `Program.cs` for each service this app calls
- Duende manages token caching and refresh automatically — do not manually acquire tokens
- `IdentityClientSettings.Name` must match the named `HttpClient` registration
- Scopes are space- or comma-separated strings