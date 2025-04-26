using Microsoft.Extensions.Logging;

namespace Razor01.Global
{
    public sealed class GlobalConfig
    {
        // Use a static instance to make the configuration globally accessible.
        public static GlobalConfig Instance
        {
            get
            {
                return lazy.Value;
            }
        }

        private static readonly Lazy<GlobalConfig> lazy = new Lazy<GlobalConfig>();
        private readonly string _env;
        private readonly string _dbType;
        private readonly string _connectionString;
        private readonly string _sqlLiteConnectionString;
        private readonly string _postgresConnectionString;
        private readonly string _redisServerName;

        public GlobalConfig()
        {
            var builder = new ConfigurationBuilder();

            var appsettingFilename = "appsettings.json";
            builder.AddJsonFile(appsettingFilename, optional: false, reloadOnChange: true);

            // Build the configuration object
            var configuration = builder.Build();
            _env = configuration.GetValue("AppConfig:Environment", "uat");

            _dbType = configuration.GetValue($"AppConfig:{_env}:DBFileName", "postgres");

            // SQLite Connection String
            var _sqliteDatabaseName = configuration.GetValue($"AppConfig:{_env}:DBFileName", "razor01.sqlite");
            _sqlLiteConnectionString = $"Data Source={AppContext.BaseDirectory}{_sqliteDatabaseName};Version=3;";

            // Postgres Connection String
            _postgresConnectionString = $"Host={configuration.GetValue($"AppConfig:{_env}:DSN", "")}:{configuration.GetValue($"AppConfig:{_env}:Port", "")};Database={configuration.GetValue($"AppConfig:{_env}:Database", "")};Username={configuration.GetValue($"AppConfig:{_env}:Username", "")};Password={configuration.GetValue($"AppConfig:{_env}:Password", "")}";

            // Redis Setup
            _redisServerName = configuration.GetValue($"AppConfig:{_env}:Redis", "");

            switch (_dbType)
            {
                case "sqlite":
                    _connectionString = _sqlLiteConnectionString;
                    break;
                default:
                    _connectionString = _postgresConnectionString;
                    break;
            }
        }

        public string Env { get { return _env; } }
        public string DBType { get { return _dbType; } }
        public string ConnectionString { get { return _connectionString; } }
        public string SQLiteConnectionstring { get { return _sqlLiteConnectionString; } }
        public string PostgresConnectionString { get { return _postgresConnectionString; } }
        public string RedisServerName { get { return _redisServerName; } }
        public string OAuthClientId {get {
            return "21b0dceb-7d9b-4ae5-ba5c-1274b0512155";
        }}
    }
}
