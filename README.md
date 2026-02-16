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
