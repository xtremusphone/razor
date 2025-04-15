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

        public readonly string _connectionString;
        public GlobalConfig() {
            var builder = new ConfigurationBuilder();

            var appsettingFilename = "appsettings.json";
            builder.AddJsonFile(appsettingFilename, optional: false, reloadOnChange: true);

            // Build the configuration object
            var configuration = builder.Build();
            var env = configuration.GetValue("AppConfig:Environment", "uat");
            var _sqliteDatabaseName = configuration.GetValue($"AppConfig:{env}:DBFileName", "razor01.sqlite");
            
            _connectionString = $"Data Source={AppContext.BaseDirectory}{_sqliteDatabaseName};Version=3;";
        }

        public string ConnectionString { get { return _connectionString; } }
    }
}
