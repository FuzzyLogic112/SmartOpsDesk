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

After the login window opens, use one of these demo accounts:

```text
dev / 123456
pm / 123456
impl / 123456
ops / 123456
```

## Configure Database and AI in the App

SmartOpsDesk does not require command-line configuration for normal use.

After logging in:

1. Click `连接设置` in the top-right area of the main window.
2. Choose a storage mode:
   - `JSON`: local file storage, no database needed.
   - `Supabase`: recommended cloud database mode.
   - `Postgres`: any PostgreSQL-compatible database.
   - `SQL Server`: Microsoft SQL Server.
3. Paste the database connection string.
4. Click `测试数据库连接`.
5. Fill in AI settings if external model analysis is needed.
6. Click `保存`.

The app saves the configuration to:

```text
%APPDATA%\SmartOpsDesk\settings.json
```

After saving, the main window automatically reloads ticket data from the selected storage.

## Use Supabase Cloud Storage

Create a Supabase project, then copy the PostgreSQL connection string from the Supabase dashboard.

Recommended steps:

1. Open Supabase and create a project.
2. Open the database connection settings.
3. Copy the PostgreSQL connection string.
4. Log in to SmartOpsDesk.
5. Click `连接设置`.
6. Select `Supabase`.
7. Paste the connection string into `Supabase / Postgres 连接串`.
8. Click `测试数据库连接`.
9. Click `保存`.

Keyword-style connection string example:

```powershell
Host=aws-0-region.pooler.supabase.com;Port=6543;Database=postgres;Username=postgres.your-project-ref;Password=YOUR_PASSWORD;SSL Mode=Require;Trust Server Certificate=true
```

URI-style connection string example:

```powershell
postgresql://postgres.your-project-ref:YOUR_PASSWORD@aws-0-region.pooler.supabase.com:6543/postgres
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

If Supabase is not configured, select `JSON` to keep using local file storage.

## Use SQL Server Storage

SQL Server is still supported as an optional storage mode.

In the app:

1. Click `连接设置`.
2. Select `SQL Server`.
3. Paste a SQL Server connection string.
4. Click `测试数据库连接`.
5. Click `保存`.

Example connection string:

```powershell
Server=YOUR_SERVER;Database=SmartOpsDesk;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True
```

## Use a Large Model API

The app works without a large model. It uses local rule-based AI by default.

To enable external model analysis:

1. Log in to SmartOpsDesk.
2. Click `连接设置`.
3. Fill in `AI Endpoint`.
4. Fill in `AI API Key`.
5. Fill in `模型名`.
6. Click `保存`.

Default endpoint example:

```text
https://api.openai.com/v1/chat/completions
```

Default model example:

```text
gpt-4o-mini
```

If AI settings are empty, the large model button will not crash the app. It records a clear message saying that the model API is not configured.

## Advanced Environment Variables

For automated deployment or CI experiments, environment variables are still supported when no UI settings file exists.

| Variable | Purpose |
|---|---|
| `SMARTOPSDESK_SUPABASE_CONNECTION` | Supabase/PostgreSQL connection string |
| `SMARTOPSDESK_POSTGRES_CONNECTION` | Generic PostgreSQL connection string |
| `SMARTOPSDESK_SQLSERVER_CONNECTION` | SQL Server connection string |
| `SMARTOPSDESK_LLM_ENDPOINT` | Chat-completions-compatible endpoint |
| `SMARTOPSDESK_LLM_API_KEY` | AI API key |
| `SMARTOPSDESK_LLM_MODEL` | AI model name |

For normal desktop use, prefer the `连接设置` window.

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
    AppSettings.cs
    AppSettingsService.cs
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
