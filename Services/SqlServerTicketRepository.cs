using Microsoft.Data.SqlClient;
using SmartOpsDesk.Models;

namespace SmartOpsDesk.Services;

public sealed class SqlServerTicketRepository(string connectionString) : ITicketRepository
{
    public List<WorkTicket> Load()
    {
        using var connection = new SqlConnection(connectionString);
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
        using var connection = new SqlConnection(connectionString);
        connection.Open();
        EnsureSchema(connection);
        using var transaction = connection.BeginTransaction();

        Execute(connection, transaction, "DELETE FROM DeploymentLogs; DELETE FROM TicketAttachments; DELETE FROM ApprovalRecords; DELETE FROM Tickets;");

        foreach (var ticket in tickets)
        {
            Execute(
                connection,
                transaction,
                """
                INSERT INTO Tickets
                (Id, Title, Requester, Department, Description, Category, Priority, SuggestedOwner, Status, AiReason, HandlingAdvice, Confidence, CreatedAt, UpdatedAt, DueAt)
                VALUES
                (@Id, @Title, @Requester, @Department, @Description, @Category, @Priority, @SuggestedOwner, @Status, @AiReason, @HandlingAdvice, @Confidence, @CreatedAt, @UpdatedAt, @DueAt);
                """,
                Parameters(ticket));

            foreach (var record in ticket.ApprovalHistory)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO ApprovalRecords (TicketId, Time, Operator, Role, Action, Comment)
                    VALUES (@TicketId, @Time, @Operator, @Role, @Action, @Comment);
                    """,
                    new("@TicketId", ticket.Id),
                    new("@Time", record.Time),
                    new("@Operator", record.Operator),
                    new("@Role", record.Role),
                    new("@Action", record.Action),
                    new("@Comment", record.Comment));
            }

            foreach (var attachment in ticket.Attachments)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO TicketAttachments (TicketId, FileName, StoredPath, UploadedBy, UploadedAt)
                    VALUES (@TicketId, @FileName, @StoredPath, @UploadedBy, @UploadedAt);
                    """,
                    new("@TicketId", ticket.Id),
                    new("@FileName", attachment.FileName),
                    new("@StoredPath", attachment.StoredPath),
                    new("@UploadedBy", attachment.UploadedBy),
                    new("@UploadedAt", attachment.UploadedAt));
            }

            foreach (var log in ticket.DeploymentLogs)
            {
                Execute(
                    connection,
                    transaction,
                    """
                    INSERT INTO DeploymentLogs (TicketId, Time, Operator, Environment, Content)
                    VALUES (@TicketId, @Time, @Operator, @Environment, @Content);
                    """,
                    new("@TicketId", ticket.Id),
                    new("@Time", log.Time),
                    new("@Operator", log.Operator),
                    new("@Environment", log.Environment),
                    new("@Content", log.Content));
            }
        }

        transaction.Commit();
    }

    private static List<WorkTicket> LoadTickets(SqlConnection connection)
    {
        using var command = new SqlCommand("SELECT * FROM Tickets ORDER BY CreatedAt DESC;", connection);
        using var reader = command.ExecuteReader();
        var tickets = new List<WorkTicket>();

        while (reader.Read())
        {
            tickets.Add(new WorkTicket
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Requester = reader.GetString(reader.GetOrdinal("Requester")),
                Department = reader.GetString(reader.GetOrdinal("Department")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Category = reader.GetString(reader.GetOrdinal("Category")),
                Priority = reader.GetString(reader.GetOrdinal("Priority")),
                SuggestedOwner = reader.GetString(reader.GetOrdinal("SuggestedOwner")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                AiReason = reader.GetString(reader.GetOrdinal("AiReason")),
                HandlingAdvice = reader.GetString(reader.GetOrdinal("HandlingAdvice")),
                Confidence = reader.GetDouble(reader.GetOrdinal("Confidence")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                DueAt = reader.GetDateTime(reader.GetOrdinal("DueAt"))
            });
        }

        return tickets;
    }

    private static void LoadApprovalRecords(SqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new SqlCommand("SELECT * FROM ApprovalRecords ORDER BY Time;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("TicketId")));
            ticket?.ApprovalHistory.Add(new ApprovalRecord
            {
                Time = reader.GetDateTime(reader.GetOrdinal("Time")),
                Operator = reader.GetString(reader.GetOrdinal("Operator")),
                Role = reader.GetString(reader.GetOrdinal("Role")),
                Action = reader.GetString(reader.GetOrdinal("Action")),
                Comment = reader.GetString(reader.GetOrdinal("Comment"))
            });
        }
    }

    private static void LoadAttachments(SqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new SqlCommand("SELECT * FROM TicketAttachments ORDER BY UploadedAt;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("TicketId")));
            ticket?.Attachments.Add(new TicketAttachment
            {
                FileName = reader.GetString(reader.GetOrdinal("FileName")),
                StoredPath = reader.GetString(reader.GetOrdinal("StoredPath")),
                UploadedBy = reader.GetString(reader.GetOrdinal("UploadedBy")),
                UploadedAt = reader.GetDateTime(reader.GetOrdinal("UploadedAt"))
            });
        }
    }

    private static void LoadDeploymentLogs(SqlConnection connection, List<WorkTicket> tickets)
    {
        using var command = new SqlCommand("SELECT * FROM DeploymentLogs ORDER BY Time;", connection);
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var ticket = tickets.FirstOrDefault(item => item.Id == reader.GetInt32(reader.GetOrdinal("TicketId")));
            ticket?.DeploymentLogs.Add(new DeploymentLog
            {
                Time = reader.GetDateTime(reader.GetOrdinal("Time")),
                Operator = reader.GetString(reader.GetOrdinal("Operator")),
                Environment = reader.GetString(reader.GetOrdinal("Environment")),
                Content = reader.GetString(reader.GetOrdinal("Content"))
            });
        }
    }

    private static void EnsureSchema(SqlConnection connection)
    {
        Execute(
            connection,
            null,
            """
            IF OBJECT_ID('Tickets', 'U') IS NULL
            CREATE TABLE Tickets (
                Id INT NOT NULL PRIMARY KEY,
                Title NVARCHAR(200) NOT NULL,
                Requester NVARCHAR(80) NOT NULL,
                Department NVARCHAR(80) NOT NULL,
                Description NVARCHAR(MAX) NOT NULL,
                Category NVARCHAR(80) NOT NULL,
                Priority NVARCHAR(20) NOT NULL,
                SuggestedOwner NVARCHAR(120) NOT NULL,
                Status NVARCHAR(20) NOT NULL,
                AiReason NVARCHAR(MAX) NOT NULL,
                HandlingAdvice NVARCHAR(MAX) NOT NULL,
                Confidence FLOAT NOT NULL,
                CreatedAt DATETIME2 NOT NULL,
                UpdatedAt DATETIME2 NOT NULL,
                DueAt DATETIME2 NOT NULL
            );

            IF OBJECT_ID('ApprovalRecords', 'U') IS NULL
            CREATE TABLE ApprovalRecords (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                TicketId INT NOT NULL,
                Time DATETIME2 NOT NULL,
                Operator NVARCHAR(80) NOT NULL,
                Role NVARCHAR(50) NOT NULL,
                Action NVARCHAR(80) NOT NULL,
                Comment NVARCHAR(MAX) NOT NULL
            );

            IF OBJECT_ID('TicketAttachments', 'U') IS NULL
            CREATE TABLE TicketAttachments (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                TicketId INT NOT NULL,
                FileName NVARCHAR(260) NOT NULL,
                StoredPath NVARCHAR(500) NOT NULL,
                UploadedBy NVARCHAR(80) NOT NULL,
                UploadedAt DATETIME2 NOT NULL
            );

            IF OBJECT_ID('DeploymentLogs', 'U') IS NULL
            CREATE TABLE DeploymentLogs (
                Id INT IDENTITY(1,1) PRIMARY KEY,
                TicketId INT NOT NULL,
                Time DATETIME2 NOT NULL,
                Operator NVARCHAR(80) NOT NULL,
                Environment NVARCHAR(80) NOT NULL,
                Content NVARCHAR(MAX) NOT NULL
            );
            """);
    }

    private static SqlParameter[] Parameters(WorkTicket ticket)
    {
        return
        [
            new("@Id", ticket.Id),
            new("@Title", ticket.Title),
            new("@Requester", ticket.Requester),
            new("@Department", ticket.Department),
            new("@Description", ticket.Description),
            new("@Category", ticket.Category),
            new("@Priority", ticket.Priority),
            new("@SuggestedOwner", ticket.SuggestedOwner),
            new("@Status", ticket.Status),
            new("@AiReason", ticket.AiReason),
            new("@HandlingAdvice", ticket.HandlingAdvice),
            new("@Confidence", ticket.Confidence),
            new("@CreatedAt", ticket.CreatedAt),
            new("@UpdatedAt", ticket.UpdatedAt),
            new("@DueAt", ticket.DueAt)
        ];
    }

    private static void Execute(SqlConnection connection, SqlTransaction? transaction, string sql, params SqlParameter[] parameters)
    {
        using var command = new SqlCommand(sql, connection, transaction);
        command.Parameters.AddRange(parameters);
        command.ExecuteNonQuery();
    }
}
