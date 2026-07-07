using Npgsql;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class PostgresTicketRepository(string connectionString) : ITicketRepository
{
    private readonly string _connectionString = NormalizeConnectionString(connectionString);

    public List<WorkTicket> Load()
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        EnsureSchema(connection);

        var tickets = LoadTickets(connection);
        if (tickets.Count == 0)
        {
            var seed = new TicketStore().Load();
            Save(seed);
            return seed;
        }

        LoadApprovalRecords(connection, tickets);
        LoadAttachments(connection, tickets);
        LoadDeploymentLogs(connection, tickets);
        return tickets;
    }

    public void Save(IEnumerable<WorkTicket> tickets)
    {
        using var connection = new NpgsqlConnection(_connectionString);
        connection.Open();
        EnsureSchema(connection);
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction, "DELETE FROM deployment_logs; DELETE FROM ticket_attachments; DELETE FROM approval_records; DELETE FROM tickets;");

        foreach (var ticket in tickets)
        {
            Execute(
                connection,
                transaction,
                """
                INSERT INTO tickets
                (id, title, requester, department, description, category, priority, suggested_owner, status, ai_reason, handling_advice, confidence, created_at, updated_at, due_at)
                VALUES
                (@id, @title, @requester, @department, @description, @category, @priority, @suggested_owner, @status, @ai_reason, @handling_advice, @confidence, @created_at, @updated_at, @due_at);
                """,
                Parameters(ticket));

            foreach (var record in ticket.ApprovalHistory)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO approval_records (ticket_id, time, operator, role, action, comment)
                    VALUES (@ticket_id, @time, @operator, @role, @action, @comment);
                    """,
                    new("ticket_id", ticket.Id),
                    new("time", record.Time),
                    new("operator", record.Operator),
                    new("role", record.Role),
                    new("action", record.Action),
                    new("comment", record.Comment));
            }

            foreach (var attachment in ticket.Attachments)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO ticket_attachments (ticket_id, file_name, stored_path, uploaded_by, uploaded_at)
                    VALUES (@ticket_id, @file_name, @stored_path, @uploaded_by, @uploaded_at);
                    """,
                    new("ticket_id", ticket.Id),
                    new("file_name", attachment.FileName),
                    new("stored_path", attachment.StoredPath),
                    new("uploaded_by", attachment.UploadedBy),
                    new("uploaded_at", attachment.UploadedAt));
            }

            foreach (var log in ticket.DeploymentLogs)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO deployment_logs (ticket_id, time, operator, environment, content)
                    VALUES (@ticket_id, @time, @operator, @environment, @content);
                    """,
                    new("ticket_id", ticket.Id),
                    new("time", log.Time),
                    new("operator", log.Operator),
                    new("environment", log.Environment),
                    new("content", log.Content));
            }
        }

        transaction.Commit();
    }

    private static List<WorkTicket> LoadTickets(NpgsqlConnection connection)
    {
        using var command = new NpgsqlCommand("SELECT * FROM tickets ORDER BY created_at DESC;", connection);
        using var reader = command.ExecuteReader();
        var tickets = new List<WorkTicket>();

        while (reader.Read())
        {
            tickets.Add(new WorkTicket
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Title = reader.GetString(reader.GetOrdinal("title")),
                Requester = reader.GetString(reader.GetOrdinal("requester")),
                Department = reader.GetString(reader.GetOrdinal("department")),
                Description = reader.GetString(reader.GetOrdinal("description")),
                Category = reader.GetString(reader.GetOrdinal("category")),
                Priority = reader.GetString(reader.GetOrdinal("priority")),
                SuggestedOwner = reader.GetString(reader.GetOrdinal("suggested_owner")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                AiReason = reader.GetString(reader.GetOrdinal("ai_reason")),
                HandlingAdvice = reader.GetString(reader.GetOrdinal("handling_advice")),
                Confidence = reader.GetDouble(reader.GetOrdinal("confidence")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("created_at")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at")),
                DueAt = reader.GetDateTime(reader.GetOrdinal("due_at"))
            });
        }

        return tickets;
    }

    private static void LoadApprovalRecords(NpgsqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new NpgsqlCommand("SELECT * FROM approval_records ORDER BY time;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("ticket_id")));
            ticket?.ApprovalHistory.Add(new ApprovalRecord
            {
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                Operator = reader.GetString(reader.GetOrdinal("operator")),
                Role = reader.GetString(reader.GetOrdinal("role")),
                Action = reader.GetString(reader.GetOrdinal("action")),
                Comment = reader.GetString(reader.GetOrdinal("comment"))
            });
        }
    }

    private static void LoadAttachments(NpgsqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new NpgsqlCommand("SELECT * FROM ticket_attachments ORDER BY uploaded_at;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("ticket_id")));
            ticket?.Attachments.Add(new TicketAttachment
            {
                FileName = reader.GetString(reader.GetOrdinal("file_name")),
                StoredPath = reader.GetString(reader.GetOrdinal("stored_path")),
                UploadedBy = reader.GetString(reader.GetOrdinal("uploaded_by")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("uploaded_at"))
            });
        }
    }

    private static void LoadDeploymentLogs(NpgsqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new NpgsqlCommand("SELECT * FROM deployment_logs ORDER BY time;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("ticket_id")));
            ticket?.DeploymentLogs.Add(new DeploymentLog
            {
                Time = reader.GetDateTime(reader.GetOrdinal("time")),
                Operator = reader.GetString(reader.GetOrdinal("operator")),
                Environment = reader.GetString(reader.GetOrdinal("environment")),
                Content = reader.GetString(reader.GetOrdinal("content"))
            });
        }
    }

    private static void EnsureSchema(NpgsqlConnection connection)
    {
        Execute(
            connection,
            null,
            """
            CREATE TABLE IF NOT EXISTS tickets (
                id INTEGER PRIMARY KEY,
                title TEXT NOT NULL,
                requester TEXT NOT NULL,
                department TEXT NOT NULL,
                description TEXT NOT NULL,
                category TEXT NOT NULL,
                priority TEXT NOT NULL,
                suggested_owner TEXT NOT NULL,
                status TEXT NOT NULL,
                ai_reason TEXT NOT NULL,
                handling_advice TEXT NOT NULL,
                confidence DOUBLE PRECISION NOT NULL,
                created_at TIMESTAMP NOT NULL,
                updated_at TIMESTAMP NOT NULL,
                due_at TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS approval_records (
                id BIGSERIAL PRIMARY KEY,
                ticket_id INTEGER NOT NULL,
                time TIMESTAMP NOT NULL,
                operator TEXT NOT NULL,
                role TEXT NOT NULL,
                action TEXT NOT NULL,
                comment TEXT NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ticket_attachments (
                id BIGSERIAL PRIMARY KEY,
                ticket_id INTEGER NOT NULL,
                file_name TEXT NOT NULL,
                stored_path TEXT NOT NULL,
                uploaded_by TEXT NOT NULL,
                uploaded_at TIMESTAMP NOT NULL
            );

            CREATE TABLE IF NOT EXISTS deployment_logs (
                id BIGSERIAL PRIMARY KEY,
                ticket_id INTEGER NOT NULL,
                time TIMESTAMP NOT NULL,
                operator TEXT NOT NULL,
                environment TEXT NOT NULL,
                content TEXT NOT NULL
            );
            """);
    }

    private static NpgsqlParameter[] Parameters(WorkTicket ticket)
    {
        return
        [
            new("id", ticket.Id),
            new("title", ticket.Title),
            new("requester", ticket.Requester),
            new("department", ticket.Department),
            new("description", ticket.Description),
            new("category", ticket.Category),
            new("priority", ticket.Priority),
            new("suggested_owner", ticket.SuggestedOwner),
            new("status", ticket.Status),
            new("ai_reason", ticket.AiReason),
            new("handling_advice", ticket.HandlingAdvice),
            new("confidence", ticket.Confidence),
            new("created_at", ticket.CreatedAt),
            new("updated_at", ticket.UpdatedAt),
            new("due_at", ticket.DueAt)
        ];
    }

    private static void Execute(NpgsqlConnection connection, NpgsqlTransaction? transaction, string sql, params NpgsqlParameter[] parameters)
    {
        using var command = new NpgsqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }

    private static string NormalizeConnectionString(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || (uri.Scheme != "postgres" && uri.Scheme != "postgresql"))
        {
            return value;
        }

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(0) ?? "");
        var password = Uri.UnescapeDataString(userInfo.ElementAtOrDefault(1) ?? "");
        var database = uri.AbsolutePath.Trim('/');
        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.Port > 0 ? uri.Port : 5432,
            Database = string.IsNullOrWhiteSpace(database) ? "postgres" : database,
            Username = username,
            Password = password,
            SslMode = SslMode.Require
        };

        return builder.ConnectionString;
    }
}
