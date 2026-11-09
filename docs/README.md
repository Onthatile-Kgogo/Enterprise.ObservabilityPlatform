
# Enterprise Observability

A reusable observability library for .NET 10 applications.

Enterprise Observability provides structured logging, distributed tracing,
performance metrics, correlation context, and ASP.NET Core middleware
integration.

## Features

- Structured logging
- Distributed tracing using .NET ActivitySource
- Performance metrics using System.Diagnostics.Metrics
- Correlation and trace context
- ASP.NET Core request telemetry
- Dependency injection integration
- Modular architecture

## Requirements

- .NET 10
- ASP.NET Core for middleware integration

## Installation

Install the package using the .NET CLI:

```bash
dotnet add package Enterprise.Observability --version 1.0.0
```

## Architecture

The NuGet package includes five assemblies:

- Enterprise.Observability.Core
- Enterprise.Observability.Logging
- Enterprise.Observability.Metrics
- Enterprise.Observability.Tracing
- Enterprise.Observability.AspNetCore

## Usage

Register the observability services in your ASP.NET Core application:

```csharp
builder.Services.AddObservability();
```

Configure the observability middleware in the HTTP request pipeline
using the middleware registration provided by the library.

## License

Licensed under the MIT License.

Copyright (c) 2026 Onthatile Kgogo

## Repository

https://github.com/Onthatile-Kgogo/Enterprise.ObservabilityPlatform
