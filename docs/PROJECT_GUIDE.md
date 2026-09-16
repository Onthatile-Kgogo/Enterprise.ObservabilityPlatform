# Enterprise Observability Platform — Project Guide

## Purpose

Build a reusable, enterprise-grade observability capability for .NET 10 applications.

This is a platform/reusability project, not another standalone business application.

The goal is to provide consistent, reusable observability capabilities that can be consumed by multiple .NET applications through clean libraries and NuGet packages.

## Technology Baseline

- .NET 10
- C#
- ASP.NET Core integration where required
- xUnit for testing
- NuGet for package distribution
- GitHub for source control

### Framework Scope

This project targets **.NET 10 only**.

Do not introduce multi-targeting or compatibility work for other .NET versions unless the project requirements explicitly change.

## Core Capabilities

The platform is intended to provide:

- Structured logging
- Metrics
- Distributed tracing
- Correlation and diagnostic context
- HTTP/API observability
- Centralized telemetry handling
- Extensible exporters/providers
- Consistent configuration
- Production-ready failure handling
- Security-conscious telemetry handling

## Solution Structure

```text
Enterprise.ObservabilityPlatform/
├── docs/
├── src/
│   ├── Enterprise.Observability.AspNetCore
│   ├── Enterprise.Observability.Core
│   ├── Enterprise.Observability.Logging
│   ├── Enterprise.Observability.Metrics
│   └── Enterprise.Observability.Tracing
├── tests/
│   ├── Enterprise.Observability.AspNetCore.Tests
│   ├── Enterprise.Observability.Core.Tests
│   ├── Enterprise.Observability.Logging.Tests
│   ├── Enterprise.Observability.Metrics.Tests
│   └── Enterprise.Observability.Tracing.Tests
├── Directory.Build.props
└── Enterprise.ObservabilityPlatform.slnx
```

## Project Responsibilities

### Enterprise.Observability.Core

Foundation of the platform.

Responsibilities:
- Core abstractions
- Shared models
- Common telemetry concepts
- Shared configuration abstractions where appropriate

Core must remain independent of ASP.NET Core and cloud/vendor-specific implementations.

### Enterprise.Observability.Logging

Responsible for logging capabilities.

Responsibilities:
- Structured logging
- Log context
- Enrichment
- Correlation information
- Provider/export pipeline abstractions
- Logging-related configuration

### Enterprise.Observability.Metrics

Responsible for metrics capabilities.

Responsibilities:
- Counters
- Histograms
- Gauges where appropriate
- Measurements
- Dimensions/tags
- Metrics collection
- Metrics export abstractions

### Enterprise.Observability.Tracing

Responsible for distributed tracing.

Responsibilities:
- Activities/spans
- Trace IDs
- Span attributes
- Context propagation
- Instrumentation abstractions
- Trace export abstractions

### Enterprise.Observability.AspNetCore

Responsible for ASP.NET Core integration.

Responsibilities:
- HTTP instrumentation
- Request/response observability
- Middleware
- Request correlation
- ASP.NET Core diagnostic integration

ASP.NET Core-specific concerns should remain here rather than leaking into Core.

## What We Are NOT Building Initially

To keep the project focused, we are not initially building:

- A sample business API
- A separate demonstration application
- An Angular dashboard
- A database-backed observability store
- Business-domain functionality
- Cloud/vendor dependencies inside Core
- Unnecessary infrastructure simply for demonstration purposes

Additional components are added only when they solve a real platform requirement.

## Architecture Principles

### Reusability
The libraries must be usable by multiple .NET applications without application-specific assumptions.

### Separation of Concerns
Each project has a clear responsibility.

### Dependency Direction
Foundational projects should not depend on higher-level application or infrastructure concerns.

### Extensibility
Providers, exporters, and integrations should be replaceable without redesigning the core platform.

### Testability
Important behaviour must be covered by automated tests.

### Production Readiness
The platform should be designed for real enterprise use rather than as a collection of demonstrations.

### Simplicity
Do not introduce abstractions, projects, dependencies, or infrastructure without a clear reason.

## Build Rule

Before building a feature, answer:

1. What real problem does this solve?
2. Which project owns the responsibility?
3. Does it belong in the reusable platform?
4. What is the simplest design that solves it?
5. How will it be tested?
6. Does it introduce an unnecessary dependency?
7. What happens if the telemetry operation fails?

If there is no clear reason to build something, do not build it.

## Failure Isolation

Observability must not unnecessarily bring down the host application.

Telemetry failures should be handled deliberately, including:
- Exporter failure
- Logging destination unavailable
- Metrics export failure
- Tracing backend unavailable
- Temporary network failures

The platform should favour graceful degradation where appropriate.

## Performance

Observability introduces overhead, therefore performance is part of the design.

Consider:
- Asynchronous processing
- Batching
- Buffering
- Backpressure
- Sampling
- Allocation reduction
- Minimal request-path overhead
- Efficient serialization
- Appropriate resource limits

Performance decisions should be based on measurable requirements rather than premature optimization.

## Security

Telemetry can contain sensitive information.

The platform must consider:
- Sensitive-data filtering
- PII protection
- Credential and token protection
- Log injection risks
- Data minimization
- Access control
- Safe exception handling
- Secure configuration

Secrets and credentials must never be written to logs or telemetry.

## Testing Strategy

Each capability should have appropriate automated tests.

Expected coverage includes:
- Unit tests
- Configuration tests
- Failure-path tests
- Integration tests where meaningful
- ASP.NET Core integration tests for HTTP-specific behaviour

Tests should verify behaviour rather than implementation details wherever practical.

## Documentation

Important architectural and behavioural decisions should be documented.

Documentation should explain:
- What the platform does
- Why major architectural decisions were made
- How consumers configure and use the platform
- How providers/exporters work
- Important operational considerations
- Security considerations

Documentation should remain aligned with the implementation.

## PR Roadmap

### PR #1 — Project Skeleton
Establish the repository and solution structure.

### PR #2 — Architecture & Core Contracts
Define the foundational contracts and shared telemetry concepts.

### PR #3 — Logging Foundation
Build the reusable logging foundation.

### PR #4 — Logging Provider / Export Pipeline
Add the provider/export mechanism required by the platform.

### PR #5 — Metrics Foundation
Build the metrics capability.

### PR #6 — Tracing Foundation
Build the distributed tracing capability.

### PR #7 — ASP.NET Core Integration
Add HTTP/request observability and ASP.NET Core integration.

### PR #8 — Correlation & Diagnostic Context
Standardize correlation and diagnostic context across telemetry.

### PR #9 — Production Hardening
Address resilience, performance, security, configuration and operational concerns.

### PR #10 — Packaging / NuGet
Prepare the reusable platform components for NuGet distribution.

### PR #11+
Additional integrations or capabilities only when justified by real requirements.

The roadmap is a guide, not a reason to build unnecessary functionality.

## Git Strategy

### main
Stable/release branch.

### development
Integration and reference branch.

### feature branches
All implementation work should be performed on focused feature branches.

Typical flow:

```text
feature/*
    ↓
development
    ↓
main
```

Do not push unfinished feature work directly to `main`.

## Definition of Done

A feature is considered complete when appropriate:

- Implementation is complete
- Public abstractions are deliberate
- Automated tests are present
- Failure paths are considered
- Configuration is documented
- Dependencies are justified
- Security considerations are addressed
- Documentation is updated
- Build succeeds
- Tests pass
- Code is ready for PR review

## Scope Guardrail

This project exists to build a meaningful, reusable enterprise observability platform.

We are **not building for the sake of building**.

When a proposed feature does not materially improve the reusable platform, it should be deferred or rejected.

The priority is:

**Meaningful capability → Clean architecture → Tests → Production readiness → Reusability**

Not:

**More projects → More code → More features**

Every addition must earn its place in the platform.
