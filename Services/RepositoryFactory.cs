namespace SmartOpsDesk.Services;

public static class RepositoryFactory
{
    public static ITicketRepository Create()
    {
        var supabaseConnectionString = Environment.GetEnvironmentVariable("SMARTOPSDESK_SUPABASE_CONNECTION");
        if (!string.IsNullOrWhiteSpace(supabaseConnectionString))
        {
            return new PostgresTicketRepository(supabaseConnectionString);
        }

        var postgresConnectionString = Environment.GetEnvironmentVariable("SMARTOPSDESK_POSTGRES_CONNECTION");
        if (!string.IsNullOrWhiteSpace(postgresConnectionString))
        {
            return new PostgresTicketRepository(postgresConnectionString);
        }

        var connectionString = Environment.GetEnvironmentVariable("SMARTOPSDESK_SQLSERVER_CONNECTION");
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            return new SqlServerTicketRepository(connectionString);
        }

        return new TicketStore();
    }

    public static string CurrentStorageName =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_SUPABASE_CONNECTION"))
            ? "Supabase/Postgres"
            : !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_POSTGRES_CONNECTION"))
                ? "Postgres"
                : !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_SQLSERVER_CONNECTION"))
                    ? "SQL Server"
                    : "JSON 本地文件";
}
