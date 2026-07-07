# SmartOpsDesk

SmartOpsDesk is a Windows desktop application for managing support tickets in OA, education administration, HR, asset management, and internal IT operation scenarios.

It helps a team record user issues, classify tickets, estimate priority, assign an owner, track SLA deadlines, keep approval history, attach screenshots, record deployment notes, and export ticket data.

## What It Does

SmartOpsDesk turns user-reported problems into trackable work tickets. It keeps the full handling process in one place, from issue registration to AI-assisted triage, approval, attachment upload, deployment notes, and final status tracking.

Typical issues include:

- The OA system cannot be opened.
- An approval workflow is stuck.
- HR reports fail to export.
- Asset records are slow to query.
- A production service times out after deployment.

## Feature Overview

- Login window with demo users
- Role-based operation checks
- Ticket create, edit, delete, search, and filter
- Rule-based AI triage
- Category, priority, owner, and handling advice suggestions
- SLA deadline calculation and timeout status
- Approval actions: approve and reject
- Attachment and screenshot upload
- Deployment and on-site handling logs
- Status flow: pending, processing, completed, suspended, rejected
- Supabase/Postgres cloud database support
- Local JSON storage fallback
- SQL Server storage support
- CSV export
- Smoke tests
- GitHub Actions CI/CD
- Windows publish artifact generation

## Tech Stack Explained Simply

### .NET 8

.NET is Microsoft's application development platform. It can be used to build desktop apps, web apps, APIs, background services, and cloud applications.

SmartOpsDesk uses .NET 8, a modern long-term support version.

### C#

C# is the main programming language used in the .NET ecosystem.

In this project, C# handles business logic such as ticket creation, AI triage, role checks, saving data, exporting CSV files, and calling optional external AI services.

### WPF

WPF stands for Windows Presentation Foundation. It is a .NET technology for building Windows desktop applications.

The login screen, main window, tables, buttons, forms, and dialogs in SmartOpsDesk are all built with WPF.

### Supabase / PostgreSQL

Supabase provides a hosted PostgreSQL database. PostgreSQL is an open-source relational database.

SmartOpsDesk can connect to Supabase through a PostgreSQL connection string. This means the app can store ticket data in the cloud without installing a local database.

### SQL Server

SQL Server is Microsoft's relational database. It is still supported as an optional storage mode.

### JSON Storage

JSON storage is used as a lightweight fallback. It allows the application to run without configuring any database.

### AI Agent

The built-in AI Agent is a rule-based triage assistant.

It reads the ticket title, department, and description, then suggests:

- Category
- Priority
- Responsible team
- Handling advice

For example, if a ticket mentions "production", "cannot open", or "all users", it is more likely to be treated as a high-priority deployment or operation issue.

### Large Model Integration

The project includes an optional large model integration.

If an API endpoint and API key are configured, SmartOpsDesk can send ticket content to a chat-completions-compatible model and save the model's response into the ticket history.

## Demo Accounts

All demo accounts use the same password:

```text
123456
```

| Username | Role | Typical Permissions |
|---|---|---|
| `dev` | Developer | Create tickets, update ticket details, upload attachments |
| `pm` | Project Manager | Approve, reject, upload attachments, write project notes |
| `impl` | Implementation Consultant | Upload customer screenshots and on-site context |
| `ops` | Operations | Write deployment and production troubleshooting logs |

## Requirements

- Windows 10 or later
- .NET 8 SDK for development
- Optional: Supabase project for cloud storage
- Optional: OpenAI-compatible chat completions API for large model analysis

## Run Locally

Clone the repository:

```powershell
git clone https://github.com/FuzzyLogic112/SmartOpsDesk.git
cd SmartOpsDesk
```

Run the app with local JSON storage:

```powershell
dotnet run
```

You can also run the included startup script on Windows:

```text
启动SmartOpsDesk.bat
```

## Use Supabase Storage

Create a Supabase project, then copy the PostgreSQL connection string from the Supabase dashboard.

Set the connection string before starting the app:

```powershell
$env:SMARTOPSDESK_SUPABASE_CONNECTION = "postgresql://postgres.your-project-ref:YOUR_PASSWORD@aws-0-region.pooler.supabase.com:6543/postgres"
dotnet run
```

You can also use a keyword-style Npgsql connection string:

```powershell
$env:SMARTOPSDESK_SUPABASE_CONNECTION = "Host=aws-0-region.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.your-project-ref;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
dotnet run
```

When Supabase is enabled, the app automatically creates these tables:

- `tickets`
- `approval_records`
- `ticket_attachments`
- `deployment_logs`

The schema is also available in:

```text
database/supabase-postgres-schema.sql
```

If `SMARTOPSDESK_SUPABASE_CONNECTION` is not set, the app uses local JSON storage.

## Use SQL Server Storage

SQL Server is still supported. Set this environment variable before running the app:

```powershell
$env:SMARTOPSDESK_SQLSERVER_CONNECTION = "Server=YOUR_SERVER;Database=SmartOpsDesk;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
dotnet run
```

## Use a Large Model API

The app works without a large model. It uses local rule-based AI by default.

To enable external model analysis, configure:

```powershell
$env:SMARTOPSDESK_LLM_ENDPOINT = "https://api.openai.com/v1/chat/completions"
$env:SMARTOPSDESK_LLM_API_KEY = "YOUR_API_KEY"
$env:SMARTOPSDESK_LLM_MODEL = "gpt-4o-mini"
dotnet run
```

If these variables are not configured, the large model button will not crash the app. Instead, it records a clear message saying that the model API is not configured.

## Run With Supabase and AI Together

```powershell
cd SmartOpsDesk

$env:SMARTOPSDESK_SUPABASE_CONNECTION = "postgresql://postgres.your-project-ref:YOUR_PASSWORD@aws-0-region.pooler.supabase.com:6543/postgres"

$env:SMARTOPSDESK_LLM_ENDPOINT = "https://api.openai.com/v1/chat/completions"
$env:SMARTOPSDESK_LLM_API_KEY = "YOUR_API_KEY"
$env:SMARTOPSDESK_LLM_MODEL = "gpt-4o-mini"

dotnet run
```

## Run Tests

```powershell
dotnet run --project tests\SmartOpsDesk.Tests\SmartOpsDesk.Tests.csproj
```

The tests currently verify:

- AI triage for high-priority production issues
- SLA deadline rules
- Demo login behavior

## Build and Publish

```powershell
dotnet build SmartOpsDesk.sln --configuration Release
.\publish.ps1
```

Published files are generated in:

```text
publish/
```

## Project Structure

```text
SmartOpsDesk/
  App.xaml
  LoginWindow.xaml
  MainWindow.xaml
  Models/
    WorkTicket.cs
  Services/
    AuthService.cs
    TicketAiAgent.cs
    LargeModelAnalyzer.cs
    TicketStore.cs
    PostgresTicketRepository.cs
    SqlServerTicketRepository.cs
    CsvExporter.cs
  database/
    supabase-postgres-schema.sql
    sqlserver-schema.sql
  tests/
    SmartOpsDesk.Tests
  .github/workflows/
    ci.yml
```

## Storage Modes

| Mode | Environment Variable | Description |
|---|---|---|
| Supabase/Postgres | `SMARTOPSDESK_SUPABASE_CONNECTION` | Stores data in a Supabase PostgreSQL database. |
| Postgres | `SMARTOPSDESK_POSTGRES_CONNECTION` | Stores data in any PostgreSQL-compatible database. |
| SQL Server | `SMARTOPSDESK_SQLSERVER_CONNECTION` | Stores data in SQL Server. |
| JSON | none | Default fallback. Stores data under `%APPDATA%\SmartOpsDesk`. |

## CI/CD

GitHub Actions workflow:

```text
.github/workflows/ci.yml
```

The workflow runs on push, pull request, and manual dispatch. It restores dependencies, builds the solution, runs smoke tests, publishes the Windows app, and uploads the published files as an artifact.

## Roadmap

- Add formal user management backed by database tables
- Add password hashing for real user accounts
- Add more detailed role permission rules
- Add ticket detail tabs for history, attachments, and deployment logs
- Add attachment preview/open action
- Add Supabase server-side pagination and filtering
- Add structured JSON response parsing for large model analysis
- Add more automated tests

