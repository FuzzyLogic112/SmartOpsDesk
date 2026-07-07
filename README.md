# SmartOpsDesk

SmartOpsDesk is a Windows desktop application for managing support tickets in OA, education administration, HR, asset management, and internal IT operation scenarios.

It helps a team record user issues, classify tickets, estimate priority, assign an owner, track SLA deadlines, keep approval history, attach screenshots, record deployment notes, and export ticket data.

## What It Does

In many internal systems, users report issues such as:

- The OA system cannot be opened.
- An approval workflow is stuck.
- HR reports fail to export.
- Asset records are slow to query.
- A production service times out after deployment.

SmartOpsDesk turns those reports into trackable work tickets. It keeps the full handling process in one place, from issue registration to AI-assisted triage, approval, attachment upload, deployment notes, and final status tracking.

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

In this project, C# handles the business logic, such as ticket creation, AI triage, role checks, saving data, exporting CSV files, and calling optional external AI services.

### WPF

WPF stands for Windows Presentation Foundation. It is a .NET technology for building Windows desktop applications.

The login screen, main window, tables, buttons, forms, and dialogs in SmartOpsDesk are all built with WPF.

### SQL Server

SQL Server is Microsoft's relational database. It is widely used in enterprise .NET systems.

SmartOpsDesk can save data to SQL Server when a connection string is configured. If no SQL Server connection is provided, it automatically falls back to local JSON storage.

### JSON Storage

JSON storage is used as a lightweight fallback. It allows the application to run without installing a database.

This is useful for local demos, development, and quick testing.

### AI Agent

The built-in AI Agent is a rule-based triage assistant.

It reads the ticket title, department, and description, then suggests:

- Category
- Priority
- Responsible team
- Handling advice

For example, if a ticket mentions "production", "cannot open", or "all users", it is more likely to be treated as a high-priority deployment or operation issue.

### Large Model Integration

The project also includes an optional large model integration.

If an API endpoint and API key are configured, SmartOpsDesk can send ticket content to a chat-completions-compatible model and save the model's response into the ticket history.

### CI/CD

CI/CD means continuous integration and continuous delivery.

This repository uses GitHub Actions to automatically:

1. Restore .NET dependencies
2. Build the solution
3. Run smoke tests
4. Publish the Windows application
5. Upload the published app as an artifact

This helps ensure that the project can be built outside the local development machine.

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
- .NET 8 Desktop Runtime for running published builds
- Optional: SQL Server or a SQL Server-compatible cloud database

## Run Locally

Clone the repository:

```powershell
git clone https://github.com/FuzzyLogic112/SmartOpsDesk.git
cd SmartOpsDesk
```

Run the app:

```powershell
dotnet run
```

You can also run the included startup script on Windows:

```text
启动SmartOpsDesk.bat
```

## Use SQL Server Storage

By default, SmartOpsDesk uses local JSON storage. To use SQL Server, set this environment variable before running the app:

```powershell
$env:SMARTOPSDESK_SQLSERVER_CONNECTION = "Server=YOUR_SERVER;Database=SmartOpsDesk;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
dotnet run
```

When SQL Server is enabled, the application creates the required tables automatically:

- `Tickets`
- `ApprovalRecords`
- `TicketAttachments`
- `DeploymentLogs`

The schema is also available in:

```text
database/sqlserver-schema.sql
```

## Use a Large Model API

The app works without a large model. It uses local rule-based AI by default.

To enable external model analysis, configure:

```powershell
$env:SMARTOPSDESK_LLM_ENDPOINT = "https://api.example.com/v1/chat/completions"
$env:SMARTOPSDESK_LLM_API_KEY = "YOUR_API_KEY"
$env:SMARTOPSDESK_LLM_MODEL = "gpt-4o-mini"
dotnet run
```

The endpoint should be compatible with the Chat Completions request and response format.

If these variables are not configured, the large model button will not crash the app. Instead, it records a clear message saying that the model API is not configured.

## Run Tests

Run the smoke tests:

```powershell
dotnet run --project tests\SmartOpsDesk.Tests\SmartOpsDesk.Tests.csproj
```

The tests currently verify:

- AI triage for high-priority production issues
- SLA deadline rules
- Demo login behavior

## Build and Publish

Build the solution:

```powershell
dotnet build SmartOpsDesk.sln --configuration Release
```

Publish the Windows app:

```powershell
.\publish.ps1
```

The published files are generated in:

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
    SqlServerTicketRepository.cs
    CsvExporter.cs
  database/
    sqlserver-schema.sql
  tests/
    SmartOpsDesk.Tests
  .github/workflows/
    ci.yml
```

## Storage Modes

SmartOpsDesk supports two storage modes:

| Mode | Description |
|---|---|
| JSON | Default mode. Stores data locally under `%APPDATA%\SmartOpsDesk`. |
| SQL Server | Enabled by `SMARTOPSDESK_SQLSERVER_CONNECTION`. Stores data in database tables. |

This design keeps the app easy to run locally while still supporting a more realistic enterprise database setup.

## CI/CD

GitHub Actions workflow:

```text
.github/workflows/ci.yml
```

The workflow runs on push, pull request, and manual dispatch. It restores dependencies, builds the solution, runs smoke tests, publishes the Windows app, and uploads the published files as an artifact.

## Roadmap

- Add a formal user management module
- Add password hashing for real user accounts
- Add more detailed role permission rules
- Add ticket detail tabs for history, attachments, and deployment logs
- Add attachment preview/open action
- Add SQL Server pagination and server-side filtering
- Add structured JSON response parsing for large model analysis
- Add more automated tests

