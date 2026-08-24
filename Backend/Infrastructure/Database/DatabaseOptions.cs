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

    public record DatabaseKey(DatabaseEngine Engine, string KeyName)
    {
        public static readonly DatabaseKey Oracle = new(DatabaseEngine.Oracle, "Oracle");
        public static readonly DatabaseKey SQL = new(DatabaseEngine.SqlServer, "SQL");

        public override string ToString() => $"{Engine}[{KeyName}]";
    }

    public enum DatabaseEngine { Oracle, SqlServer }
}
