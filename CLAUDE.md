# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

This project runs on WSL2 but uses a Windows-side .NET SDK. All dotnet commands must go through `cmd.exe`:

```bash
# Build
cmd.exe /c "dotnet build BioTime.csproj"

# Run (http://localhost:5286)
cmd.exe /c "dotnet run --project BioTime.csproj"

# Restore packages
cmd.exe /c "dotnet restore BioTime.csproj"
```

There are no tests in this project currently.

## Architecture

ASP.NET Core 9.0 Web API that acts as a proxy/wrapper around the [BioTime](https://www.zkteco.com/) (ZKTeco) biometric attendance system REST API. It exposes simplified CRUD endpoints for employee management.

### Request Flow

```
Client → BioTimeController → IBioTimeService → BioTime REST API (personnel/api/employees/)
```

### Authentication

`BioTimeService` authenticates against BioTime's `jwt-api-token-auth/` endpoint. The JWT token is cached in-memory and automatically refreshed on 401 via `SendWithRetryAsync` (single retry pattern). The token uses the `JWT` scheme (not `Bearer`).

### Key Conventions

- **JSON serialization**: DTOs use `[JsonPropertyName("snake_case")]` for deserialization from BioTime. Outbound serialization uses `JsonNamingPolicy.SnakeCaseLower`.
- **Custom JsonConverters**: `DepartmentDtoConverter` and `PositionDtoConverter` handle BioTime's polymorphic responses where `department`/`position` fields can be either an integer ID or a full nested object.
- **Error handling**: Controller actions catch `HttpRequestException` and return 502 with error details. The service layer throws on non-success status codes.
- **Service lifetime**: `BioTimeService` is registered as **Singleton** (token state is shared across requests).

### Configuration

BioTime connection settings are in `appsettings.json` under the `BioTime` section, mapped to `BioTimeSettings`:
- `BaseUrl` - BioTime server URL
- `Username` / `Password` - API credentials
