using Serilog;
namespace Factory
{
    public class LoggerService
    {
        public static void InitLogService(string LogFolder = "", string logLevel="debug")
        {
            
            if (string.IsNullOrEmpty(LogFolder))
                LogFolder = AppContext.BaseDirectory;


            var loggerConfig = new LoggerConfiguration();
            if (logLevel.Equals("debug", StringComparison.OrdinalIgnoreCase))
            {
                loggerConfig.MinimumLevel.Debug();
            }
            else if (logLevel.Equals("information", StringComparison.OrdinalIgnoreCase))
            {
                loggerConfig.MinimumLevel.Information();
            }
            else if (logLevel.Equals("warning", StringComparison.OrdinalIgnoreCase))
            {
                loggerConfig.MinimumLevel.Warning();
            }
            else
            {
                loggerConfig.MinimumLevel.Error();
            }
            loggerConfig.WriteTo.File(string.Format("{0}/logs/log_.txt", LogFolder), rollingInterval: RollingInterval.Day);
            loggerConfig.WriteTo.Console();

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
