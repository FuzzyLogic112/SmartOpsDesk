namespace SmartOpsDesk.Services;

public static class RepositoryFactory
{
    public static ITicketRepository Create()
    {
        var connectionString = Environment.GetEnvironmentVariable("SMARTOPSDESK_SQLSERVER_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return new SqlServerTicketRepository(connectionString);
        }

        return new TicketStore();
    }

    public static string CurrentStorageName =>
        string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_SQLSERVER_CONNECTION"))
            ? "JSON 本地文件"
            : "SQL Server";
}
