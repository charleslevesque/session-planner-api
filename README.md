# SessionPlanner API

## Overview

SessionPlanner is a REST API built with C# and .NET 10.

The project provides a structured and extensible foundation for managing session-related data through a clean and maintainable API architecture.

---

## Tech Stack

- **Language:** C#
- **Framework:** .NET 10 (Preview)
- **Database:** SQLite
- **CI:** GitHub Actions

---
## Getting Started

### Prerequisites

- .NET 10 SDK (Preview)
- Git
- EF Core CLI tool

If you don't have the EF tool installed:

```bash
dotnet tool install --global dotnet-ef
```
---
### Clone the Repository

```bash
git clone https://github.com/charleslevesque/session-planner-api.git
cd session-planner-api
```
---
### Restore Dependencies

```bash
dotnet restore
```
---
### Database Setup (SQLite + EF Core)

```bash
dotnet ef database update -p src/SessionPlanner.Infrastructure -s src/SessionPlanner.Api
```
This will:
- Apply migrations
- Create the local SQLite database
- Generate required tables

---
### Run the API:

```bash
dotnet run --project src/SessionPlanner.Api
```
The API will start locally.

---

## Testing

The project includes comprehensive unit and integration tests using **xUnit** and **FluentAssertions**.

### Run All Tests

```bash
dotnet test
```

### Run Tests with Code Coverage

```bash
dotnet test --settings tests/coverlet.runsettings --collect:"XPlat Code Coverage"
```

### Generate Coverage Report (HTML)

First, install the report generator tool:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
```

Then generate the report:

```bash
reportgenerator -reports:"tests/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
```

Open `coveragereport/index.html` in your browser to view the detailed coverage report.

### Test Projects

| Project | Type | Description |
|---------|------|-------------|
| `SessionPlanner.Tests.Unit` | Unit Tests | Tests for mappings, DTOs, and entities |
| `SessionPlanner.Tests.Integration` | Integration Tests | Tests for API controllers using in-memory database |

### Useful Test Commands

| Command | Description |
|---------|-------------|
| `dotnet test --filter "ClassName=LaboratoriesControllerTests"` | Run tests for a specific class |
| `dotnet test --verbosity detailed` | Run with detailed output |
| `dotnet test --no-build` | Run without rebuilding |
| `dotnet test tests/SessionPlanner.Tests.Unit` | Run only unit tests |
| `dotnet test tests/SessionPlanner.Tests.Integration` | Run only integration tests |
