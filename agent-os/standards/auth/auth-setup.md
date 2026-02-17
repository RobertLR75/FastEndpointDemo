# Authentication Setup

Two mutually exclusive patterns — use exactly one per service:

**UI apps (Blazor/MVC):** OIDC Authorization Code Flow + cookie session
```csharp
// Program.cs
builder.ConfigureClientAuthentication(new OpenIdConnectSettings { ... });
app.UseClientAuthentication(settings);
```

**API services (FastEndpoints):** JWT Bearer validation
```csharp
// Program.cs
builder.ConfigureServerAuthentication(new ServerAuthenticationSettings { Authority = "..." });
// app.UseAuthentication() + UseAuthorization() are called inside UseEndpoints()
```

- Never use both in the same service
- Login/logout routes (`/authentication/login`, `/authentication/logout`) are registered automatically for UI apps
