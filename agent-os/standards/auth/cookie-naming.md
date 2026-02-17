# Cookie Naming

Cookie names are derived from `OpenIdConnectSettings.Name` (the application/service name).

| Purpose | Cookie name |
|---|---|
| Authentication scheme | `{Name}_Cookie` |
| Actual browser cookie | `{Name}_AuthCookie` |

Example with `Name = "MyApp"`:
- Scheme: `MyApp_Cookie`
- Browser cookie: `MyApp_AuthCookie`

- Set `OpenIdConnectSettings.Name` to the application name (e.g. `"OrderPortal"`)
- Both names are set automatically by `ConfigureClientAuthentication` — do not set them manually
