using System.Collections.Generic;

namespace Backend.Infrastructure.Database
{
    public class DatabaseConfig
    {
        public List<OracleConnectionOptions>? Oracle { get; set; }
        public List<SqlServerConnectionOptions>? SqlServer { get; set; }
    }

    public class OracleConnectionOptions
    {
        public string Alias { get; set; } = string.Empty;
        public string TnsName { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string BuildConnectionString() =>
            $"User Id={User};Password={Password};Data Source={TnsName};";
    }

    public class SqlServerConnectionOptions
    {
        public string Alias { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Database { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;

        public string BuildConnectionString() =>
            $"Server={Host};Database={Database};User Id={User};Password={Password};TrustServerCertificate=True;";
    }

    public record DatabaseKey(DatabaseEngine Engine, int Index = 0)
    {
        public static readonly DatabaseKey TC = new(DatabaseEngine.Oracle, 0);
        public static readonly DatabaseKey CBS = new(DatabaseEngine.Oracle, 1);
        public static readonly DatabaseKey CBS_CORTO = new(DatabaseEngine.Oracle, 2);
        public static readonly DatabaseKey SQL = new(DatabaseEngine.SqlServer, 0);

        public override string ToString() => $"{Engine}[{Index}]";
    }

    public enum DatabaseEngine { Oracle, SqlServer }
}
