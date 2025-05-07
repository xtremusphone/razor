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
        private readonly string _RPNRSSURL;
        private readonly string _OAuthURL;
        private readonly string _OAuthClientId;
        private readonly string _OAuthRedirectURL;

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

            _RPNRSSURL = configuration.GetValue($"AppConfig:{_env}:RPNRSSURL", "");
            _OAuthURL = configuration.GetValue($"AppConfig:{_env}:OAuthURL", "");
            _OAuthClientId = configuration.GetValue($"AppConfig:{_env}:OAuthClientId", "");
            _OAuthRedirectURL = configuration.GetValue($"AppConfig:{_env}:OAuthRedirectURL", "");
        }

        public string Env { get { return _env; } }
        public string DBType { get { return _dbType; } }
        public string ConnectionString { get { return _connectionString; } }
        public string SQLiteConnectionstring { get { return _sqlLiteConnectionString; } }
        public string PostgresConnectionString { get { return _postgresConnectionString; } }
        public string RedisServerName { get { return _redisServerName; } }
        public string OAuthURL { get { return _OAuthURL; }}
        public string OAuthClientId {get { return _OAuthClientId; }}
        public string OAuthRedirectURL {get {return _OAuthRedirectURL;}}
        public string RPNRSSURL {get { return _RPNRSSURL; }}
    }
}
