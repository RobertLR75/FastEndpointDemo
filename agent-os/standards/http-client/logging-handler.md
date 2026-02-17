# LoggingHandler

`LoggingHandler` is a `DelegatingHandler` that logs full HTTP request and response bodies to `Console.WriteLine`. Attach in development only.

```csharp
// Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddTransient<LoggingHandler>();
    builder.Services.AddHttpClient(clientName)
        .AddHttpMessageHandler<LoggingHandler>();
}
```

- Never register in production — outputs raw payloads including tokens and sensitive data
- Enabled globally in development for all registered HTTP clients
- Output goes to stdout (`Console.WriteLine`) not to the structured logger