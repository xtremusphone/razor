using Microsoft.Win32;
using Serilog;
using System.Diagnostics;

namespace Factory
{
    internal sealed class WinRegistryFactory : IDisposable
    {
        const string baseServiceRegistry = @"HKEY_CURRENT_USER";
        const string serviceKeyName = @"Software\Olif";
        const string keyPath = $@"{baseServiceRegistry}\{serviceKeyName}";
        private bool disposedValue;

        internal string GetValue(string keyName)
        {
            try
            {
                // Read the value from the registry
                var value = Registry.GetValue(keyPath, keyName, "");
                if (value != null)
                {
                    return value.ToString();
                }
                else
                    return "";
            }
            catch(Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                return "";
            }
           
        }

        internal bool SetValue(string keyName, string keyValue)
        {
            try
            {
                // Set the value in the registry
                Registry.SetValue(keyPath, keyName, keyValue, RegistryValueKind.String);
                return true;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                return false;
            }
        }

        internal bool DeleteKeyPath()
        {
            try
            {
                Log.Debug("{funcName}: {serviceKeyName}", "DeleteKeyPath", serviceKeyName);
                Registry.CurrentUser.DeleteSubKey(serviceKeyName);
                return true;
            }
            catch (Exception ex)
            {
                var funcName = string.Format("{0} : {1}", new StackFrame().GetMethod().DeclaringType.FullName, System.Reflection.MethodBase.GetCurrentMethod().Name);
                Log.Error("{funcName}: {error}", funcName, ex.Message);
                return false;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WinRegistryFactory()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
    
}
