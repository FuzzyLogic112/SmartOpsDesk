namespace SmartOpsDesk.Services;

public static class RepositoryFactory
{
    public static ITicketRepository Create()
    {
        if (AppSettingsService.Exists)
        {
            return Create(AppSettingsService.Load());
        }

        return CreateFromEnvironment();
    }

    public static ITicketRepository Create(AppSettings settings)
    {
        if (settings.StorageMode == "JSON")
        {
            return new TicketStore();
        }

        if (settings.StorageMode == "Supabase" && !string.IsNullOrWhiteSpace(settings.SupabaseConnection))
        {
            return new PostgresTicketRepository(settings.SupabaseConnection);
        }

        if (settings.StorageMode == "Postgres" && !string.IsNullOrWhiteSpace(settings.PostgresConnection))
        {
            return new PostgresTicketRepository(settings.PostgresConnection);
        }

        if (settings.StorageMode == "SQL Server" && !string.IsNullOrWhiteSpace(settings.SqlServerConnection))
        {
            return new SqlServerTicketRepository(settings.SqlServerConnection);
        }

        return new TicketStore();
    }

    private static ITicketRepository CreateFromEnvironment()
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
        AppSettingsService.Exists
            ? CurrentStorageNameFor(AppSettingsService.Load())
            : CurrentEnvironmentStorageName;

    public static string CurrentStorageNameFor(AppSettings settings)
    {
        if (settings.StorageMode == "JSON")
        {
            return "JSON 本地文件";
        }

        if (settings.StorageMode == "Supabase" && !string.IsNullOrWhiteSpace(settings.SupabaseConnection))
        {
            return "Supabase/Postgres";
        }

        if (settings.StorageMode == "Postgres" && !string.IsNullOrWhiteSpace(settings.PostgresConnection))
        {
            return "Postgres";
        }

        if (settings.StorageMode == "SQL Server" && !string.IsNullOrWhiteSpace(settings.SqlServerConnection))
        {
            return settings.StorageMode;
        }

        return "JSON 本地文件";
    }

    private static string CurrentEnvironmentStorageName =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_SUPABASE_CONNECTION"))
            ? "Supabase/Postgres"
            : !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_POSTGRES_CONNECTION"))
                ? "Postgres"
                : !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SMARTOPSDESK_SQLSERVER_CONNECTION"))
                    ? "SQL Server"
                    : "JSON 本地文件";
}
