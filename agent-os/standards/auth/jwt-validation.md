# JWT Validation

API services validate JWTs with these settings (configured via `ConfigureServerAuthentication`):

```csharp
new ServerAuthenticationSettings { Authority = "https://your-identity-server" }
```

Validation rules:
- `ValidateIssuer = true` — issuer must match Authority
- `ValidateAudience = false` — intentional; services do not use audience claims
- `NameClaimType = "name"` — maps OIDC `name` claim to `ClaimTypes.Name`
- `RoleClaimType = "role"` — maps OIDC `role` claim to `ClaimTypes.Role`

- Do not enable audience validation — tokens are not issued with audience claims
- `Authority` must be set; `ConfigureServerAuthentication` throws if null or empty
